using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageGalleryApp.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool b)
            {
                flag = b;
            }
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue && nullable.Value;
            }
            else if (value is int i)
            {
                flag = i > 0;
            }

            if (parameter != null && parameter.ToString()?.ToLowerInvariant() == "invert")
            {
                flag = !flag;
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                var flag = visibility == Visibility.Visible;
                if (parameter != null && parameter.ToString()?.ToLowerInvariant() == "invert")
                {
                    flag = !flag;
                }
                return flag;
            }
            return false;
        }
    }
}