using System.Globalization;
using System.Windows.Data;

namespace AuroraVisionLauncher.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public Type EnumType { get; set; } = default!;

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString && value is not null)
        {
            if (Enum.IsDefined(EnumType!, value))
            {
                var enumValue = Enum.Parse(EnumType!, enumString);

                return enumValue.Equals(value);
            }
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(EnumType!, enumString);
        }

        return null;
    }
}
