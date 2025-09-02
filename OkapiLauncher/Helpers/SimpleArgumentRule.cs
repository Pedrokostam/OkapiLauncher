using System.Globalization;
using System.Windows.Controls;

namespace OkapiLauncher.Helpers;
public class SimpleArgumentRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string s)
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentHasToBeString);
        }
        s = s.Trim();
        if (s.Contains(' ', StringComparison.Ordinal))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoWhitespace);
        }
        if (s.Contains('"', StringComparison.Ordinal))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoDoubleQuotes);
        }
        if (s.Contains('\'', StringComparison.Ordinal))
        {
            return new ValidationResult(isValid: false, Properties.Resources.ValidationArgumentNoSingleQuotes);
        }
        return ValidationResult.ValidResult;
    }
}
