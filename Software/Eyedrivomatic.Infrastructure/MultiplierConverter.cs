using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class MultiplierConverter : IValueConverter
    {
        public object Convert([AllowNull]object value, Type targetType, [AllowNull]object parameter, CultureInfo culture)
        {
            var converter = new DoubleConverter();
            var multiplier = parameter as int? ?? parameter as double? ?? (double) (converter.ConvertFrom(parameter) ?? 1d);
            var dVal = value as int? ?? value as double? ?? (double) (converter.ConvertFrom(value) ?? 0d);
            return multiplier * dVal;
        }

        public object ConvertBack([AllowNull]object value, Type targetType, [AllowNull]object parameter, CultureInfo culture)
        {
            var converter = new DoubleConverter();
            var multiplier = parameter as int? ?? parameter as double? ?? (double)(converter.ConvertFrom(parameter) ?? 1d);
            var dVal = value as int? ?? value as double? ?? (double)(converter.ConvertFrom(value) ?? 0d);
            return dVal /multiplier;
        }
    }
}
