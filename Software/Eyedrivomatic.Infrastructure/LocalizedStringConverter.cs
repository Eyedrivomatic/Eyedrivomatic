using System;
using System.Globalization;
using System.Resources;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class LocalizedStringConverter : IValueConverter
    {
        public string ResourcePattern { get; set; } = "{0}";

        public ResourceManager ResourceManager { get; set; }

        public object Convert(object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            var resourceName = $"{ResourcePattern}{value}";
            var result = ResourceManager?.GetString(resourceName);
            if (string.IsNullOrWhiteSpace(result)) return value?.ToString() ?? string.Empty;
            return result;
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}