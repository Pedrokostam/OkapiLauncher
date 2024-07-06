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

namespace AuroraVisionLauncher.Converters;
public class CommandlineToFormattedTextConverter : IValueConverter
{
    private static readonly Regex Tokenizer = new("(\"[^\"]+\"|\\S+)");
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string input)
        {
            return DependencyProperty.UnsetValue;
        }

        var para = new Paragraph();
        para.FontFamily = new System.Windows.Media.FontFamily("Consolas");

        // Example logic to split and format parts of the text
        var parts = Tokenizer.Matches(input).Select(x => x.Value).ToList();

        if (parts.Count == 0)
        {
            return new FlowDocument(para);
        }

        var exeRun = new Run(parts[0]);
        exeRun.Foreground = Brushes.Green;

        para.Inlines.Add(exeRun);

        foreach (var part in parts.Skip(1))
        {
            para.Inlines.Add(new Run(" "));
            var run = new Run(part);
            if (part.StartsWith('-'))
            {
                run.FontStyle = FontStyles.Italic;
                run.Foreground = Brushes.Cyan;
            }
            else
            {
                run.FontWeight = FontWeights.Bold;
                run.Foreground = Brushes.Orange;
            }
            para.Inlines.Add(run);
        }

        return new FlowDocument(para);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
