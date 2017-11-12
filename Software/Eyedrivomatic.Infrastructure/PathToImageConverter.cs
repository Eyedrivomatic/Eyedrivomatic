using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            var converter = new ImageSourceConverter();
            return converter.ConvertFromString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
