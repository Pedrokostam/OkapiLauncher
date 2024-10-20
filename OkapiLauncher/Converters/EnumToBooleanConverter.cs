using System.Globalization;
using System.Windows.Data;

namespace OkapiLauncher.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public Type EnumType { get; set; } = default!;

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString && value is not null)
        {
            if (Enum.IsDefined(EnumType!, value))
            {
                bool isNegated = enumString.StartsWith('!');
                if (isNegated)
                {
                    enumString = enumString[1..];
                }
                var enumValue = Enum.Parse(EnumType!, enumString);

                return enumValue.Equals(value) ^ isNegated;
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
