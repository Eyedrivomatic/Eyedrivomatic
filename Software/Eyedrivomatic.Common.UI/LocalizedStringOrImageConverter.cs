//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Common.UI
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
        public bool ConvertToTranslation { get; set; }

        [AllowNull] public ResourceManager ResourceManager { get; set; }
        [AllowNull] public FrameworkElement FrameworkElement { get; set; }
        

        [return: AllowNull]
        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            var resourceName = string.Format(ResourcePattern, value).Replace(" ", "");
            var image = FindImage(resourceName);
            if (image != null) return image;

            var frameworkResource = FindFrameworkResource(resourceName);
            if (frameworkResource != null) return frameworkResource;

            return ConvertToTranslation 
                ? Translate.TranslationFor(resourceName, value?.ToString()) as object
                : Translate.Key(resourceName, value?.ToString());
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
            return value?.ToString();
        }
    }
}