using System.Globalization;

namespace AritmaEnvanter.Mobile.Converters
{
    public class NotNullToBoolConverter : IValueConverter
    {
        public static NotNullToBoolConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result = value != null;
            if (parameter?.ToString() == "invert")
                return !result;
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
