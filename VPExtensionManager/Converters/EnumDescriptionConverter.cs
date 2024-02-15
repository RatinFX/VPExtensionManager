using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace VPExtensionManager.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || !value.GetType().IsEnum)
            return value;

        var enumValue = (Enum)value;
        var field = enumValue.GetType().GetField(enumValue.ToString());

        if (field == null)
            return value;

        // Check Description attribute
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        if (attribute == null)
            return value;

        // Check in Resources
        var text = Properties.Resources.ResourceManager.GetString(attribute.Description, CultureInfo.InvariantCulture);

        if (text == null)
            return value;

        return $"{text} ({value})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
