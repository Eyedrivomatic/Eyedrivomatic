using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    /// <summary>
    /// Looks for a file in Images/{ResourcePattern}.png
    /// If none is found, looks for a localized resource named {ResourcePattern}
    /// If none is found, looks for a localized string {ResourcePattern}
    /// If none is found, returns the original value.
    /// </summary>
    public class LocalizedStringOrImageConverter : IValueConverter
    {
        public string ResourcePattern { get; set; } = "{0}";

        [AllowNull] public ResourceManager ResourceManager { get; set; }
        [AllowNull] public FrameworkElement FrameworkElement { get; set; }
        
        [return: AllowNull]
        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            var resourceName = string.Format(ResourcePattern, value);
            var image = FindImage(resourceName);
            if (image != null) return image;

            var frameworkResource = FindFrameworkResource(resourceName);
            if (frameworkResource != null) return frameworkResource;

            return Translate.TranslationFor(resourceName, value?.ToString());
        }

        [return: AllowNull]
        public ImageSource FindImage(string value)
        {
            var converter = new ImageSourceConverter();
            var imagePath = $"Images/{value}.png";
            try
            {
                if (!File.Exists(imagePath)) return null;
                return converter.ConvertFrom(imagePath) as ImageSource;

            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        [return: AllowNull]
        public object FindFrameworkResource(string value)
        {
            return FrameworkElement?.TryFindResource(value);
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}