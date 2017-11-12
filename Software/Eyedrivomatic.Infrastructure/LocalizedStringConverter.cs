using System;
using System.Globalization;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class LocalizedStringConverter : IValueConverter
    {
        public string ResourcePattern { get; set; } = "{0}";

        public object Convert(object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            var resourceName = $"{ResourcePattern}{value}";
            return Translate.TranslationFor(resourceName, value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, [AllowNull] CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}