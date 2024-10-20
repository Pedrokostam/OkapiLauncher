using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Windows.Media;
using AuroraVisionLauncher.Properties;

namespace AuroraVisionLauncher.Converters;
public class CommandlineToFormattedTextConverter : IValueConverter
{
    private static readonly Regex Tokenizer = new(@"(""[^""]+""|\S+)", RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(200));
    private static readonly Regex ArgumentFinder = new(@"(?<=^)-*-(?=\w)", RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(200));
    private const char WordJoiner = '\u2060';
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string input)
        {
            return DependencyProperty.UnsetValue;
        }

        var para = new Paragraph();
        para.TextAlignment = TextAlignment.Left;

        // Example logic to split and format parts of the text
        var parts = Tokenizer.Matches(input).Select(x => x.Value).ToList();

        if (parts.Count == 0)
        {
            return new FlowDocument(para);
        }

        var exeRun = new Run(parts[0]);
        exeRun.Style = Application.Current.Resources["CommandlineAppStyle"] as Style;

        para.Inlines.Add(exeRun);

        foreach (var part in parts.Skip(1))
        {
            para.Inlines.Add(new Run(" "));
            var text = ArgumentFinder.Replace(part, $"$0{WordJoiner}"); // add word joiner after hyphens
            var run = new Run(text);
            if (text.StartsWith('-'))
            {
                run.Style = Application.Current.Resources["CommandlineParameterStyle"] as Style;
            }
            else
            {
                run.Style = Application.Current.Resources["CommandlineValueStyle"] as Style;
            }
            para.Inlines.Add(run);
        }
        var doc = new FlowDocument(para)
        {
            PagePadding = new Thickness(0),
            TextAlignment = TextAlignment.Left,
            LineHeight = double.NaN,
            IsOptimalParagraphEnabled = false,
            IsColumnWidthFlexible = true,
            IsHyphenationEnabled = false,

        };
        return doc;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not FlowDocument doc)
        {
            return null;
        }
        string result;
        if (parameter is System.Windows.Documents.TextSelection selection)
        {
            result = selection.Text;
        }
        else
        {
            result = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        }
        return result.Replace(WordJoiner.ToString(), "",StringComparison.Ordinal);
    }
}
