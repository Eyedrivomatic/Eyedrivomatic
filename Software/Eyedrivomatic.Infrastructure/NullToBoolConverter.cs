using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class NullToBoolConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty ValueIfNullProperty = DependencyProperty.Register(
            nameof(ValueIfNull), typeof(bool), typeof(NullToBoolConverter), new PropertyMetadata(default(bool)));

        public bool ValueIfNull
        {
            get => (bool) GetValue(ValueIfNullProperty);
            set => SetValue(ValueIfNullProperty, value);
        }


        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            return value == null ? ValueIfNull : !ValueIfNull;
        }

        public object ConvertBack(object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new NullToBoolConverter();
        }
    }
}
