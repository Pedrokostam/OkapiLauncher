using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.Resources.Core;

namespace AuroraVisionLauncher.Helpers;
public class SimpleArgumentRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string s)
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentHasToBeString);
        }
        s = s.Trim();
        if (s.Contains(' '))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoWhitespace);
        }
        if (s.Contains('"'))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoDoubleQuotes);
        }
        if (s.Contains('\''))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoSingleQuotes);
        }
        return ValidationResult.ValidResult;
    }
}
