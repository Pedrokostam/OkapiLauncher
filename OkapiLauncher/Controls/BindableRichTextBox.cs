using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OkapiLauncher.Controls;
/// <summary>
/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///
/// Step 1a) Using this custom control in a XAML file that exists in the current project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:OkapiLauncher.Controls"
///
///
/// Step 1b) Using this custom control in a XAML file that exists in a different project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:OkapiLauncher.Controls;assembly=OkapiLauncher.Controls"
///
/// You will also need to add a project reference from the project where the XAML file lives
/// to this project and Rebuild to avoid compilation errors:
///
///     Right click on the target project in the Solution Explorer and
///     "Add Reference"->"Projects"->[Browse to and select this project]
///
///
/// Step 2)
/// Go ahead and use your control in the XAML file.
///
///     <MyNamespace:BindableRichTextBox/>
///
/// </summary>
public class BindableRichTextBox : RichTextBox
{
    private class DefaultConverter : IValueConverter
    {
        public static readonly DefaultConverter Instance = new DefaultConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string input)
            {
                return DependencyProperty.UnsetValue;
            }
            var para = new Paragraph();
            para.Inlines.Add(new Run(input));
            return new FlowDocument(para);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not FlowDocument doc)
            {
                return null;
            }
            if (parameter is System.Windows.Documents.TextSelection selection)
            {
                return selection.Text;
            }
            return new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        }
    }
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register("Document", typeof(FlowDocument),
        typeof(BindableRichTextBox), new FrameworkPropertyMetadata
        (defaultValue: null, new PropertyChangedCallback(OnDocumentChanged)));


    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string),
        typeof(BindableRichTextBox), new FrameworkPropertyMetadata
        (defaultValue: null, new PropertyChangedCallback(OnTextChanged)));

    public static readonly DependencyProperty FormatterProperty =
        DependencyProperty.Register("Formatter", typeof(IValueConverter),
        typeof(BindableRichTextBox), new FrameworkPropertyMetadata
        (defaultValue: null, new PropertyChangedCallback(OnFormatterChanged)));

    public BindableRichTextBox() : base()
    {
        ContextMenu = null;
    }
    //public new FlowDocument Document
    //{
    //    get
    //    {
    //        return (FlowDocument)this.GetValue(DocumentProperty);
    //    }

    //    set
    //    {
    //        if (value is not null && (Text is not null || Formatter is not null))
    //        {
    //            throw new InvalidOperationException("Cannot set both Document and Text/Formatter properties");
    //        }
    //        this.SetValue(DocumentProperty, value);
    //    }
    //}
    public string Text
    {
        get
        {
            return (string)this.GetValue(TextProperty);
        }

        set
        {
            //if (value is not null && Document is not null)
            //{
            //    throw new InvalidOperationException("Cannot set both Document and Text/Formatter properties");
            //}
            this.SetValue(DocumentProperty, value);
        }
    }
    public IValueConverter Formatter
    {
        get
        {
            return (IValueConverter)this.GetValue(FormatterProperty);
        }

        set
        {
            //if (value is not null && Document is not null)
            //{
            //    throw new InvalidOperationException("Cannot set both Document and Text/Formatter properties");
            //}
            this.SetValue(DocumentProperty, value);
        }
    }
    private static FlowDocument? FormatText(string? text, IValueConverter? converter)
    {
        text ??= "";
        converter ??= DefaultConverter.Instance;
        return converter.Convert(text, typeof(FlowDocument), null, CultureInfo.CurrentCulture) as FlowDocument;
    }

    public static void OnDocumentChanged(DependencyObject obj,
        DependencyPropertyChangedEventArgs args)
    {
        BindableRichTextBox rtb = (BindableRichTextBox)obj;
        ((RichTextBox)rtb).Document = (FlowDocument)args.NewValue;
    }
    public static void OnTextChanged(DependencyObject obj,
    DependencyPropertyChangedEventArgs args)
    {
        BindableRichTextBox rtb = (BindableRichTextBox)obj;
        ((RichTextBox)rtb).Document = FormatText(args.NewValue as string, rtb.Formatter) ?? new();
    }
    public static void OnFormatterChanged(DependencyObject obj,
    DependencyPropertyChangedEventArgs args)
    {
        BindableRichTextBox rtb = (BindableRichTextBox)obj;
        ((RichTextBox)rtb).Document = FormatText(rtb.Text, args.NewValue as IValueConverter) ?? new();
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            var text = (string)Formatter.ConvertBack(Document, typeof(string), Selection, null);
            Clipboard.SetText(text);
            e.Handled = true;
            return;
        }
        base.OnPreviewKeyDown(e);
    }

}
