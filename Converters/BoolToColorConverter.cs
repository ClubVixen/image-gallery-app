using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageGalleryApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue && isTrue)
            {
                var colorName = parameter as string ?? "Green";
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
