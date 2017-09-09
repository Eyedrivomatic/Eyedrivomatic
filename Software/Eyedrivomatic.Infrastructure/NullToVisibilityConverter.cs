using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public Visibility VisibilityIfNotNull { get; set; } = Visibility.Visible;
        public Visibility VisibilityIfNull { get; set; } = Visibility.Hidden;

        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            return value == null ? VisibilityIfNull : VisibilityIfNotNull;
        }

        public object ConvertBack(object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}