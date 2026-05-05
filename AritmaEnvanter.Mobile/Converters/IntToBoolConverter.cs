using System.Globalization;

namespace AritmaEnvanter.Mobile.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                bool result = intValue > 0;
                if (parameter?.ToString() == "invert")
                    return !result;
                return result;
            }
            if (value is string stringValue)
            {
                bool result = !string.IsNullOrWhiteSpace(stringValue);
                if (parameter?.ToString() == "notempty")
                    return result;
                if (parameter?.ToString() == "invert")
                    return !result;
                return result;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
