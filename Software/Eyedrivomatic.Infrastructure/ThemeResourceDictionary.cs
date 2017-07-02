using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Eyedrivomatic.Infrastructure
{
    [Localizability(LocalizationCategory.Ignore)]
    [Ambient]
    [UsableDuringInitialization(true)]
    public class ThemeResourceDictionary : ResourceDictionary
    {
        public string ThemeName { get; set; }

        public ThemeResourceDictionary()
        {}

        public ThemeResourceDictionary(Uri source)
        {
            Source = source;
        }
    }

    public class ThemeResourceDictionaryConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType.IsAssignableFrom(typeof(ResourceDictionary))) return true;
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsAssignableFrom(typeof(ResourceDictionary))) return true;
            if (destinationType == typeof(string)) return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is ResourceDictionary sourceDictionary)
            {
                return new ThemeResourceDictionary(sourceDictionary.Source);
            }
            if (value is string source)
            {
                return new ThemeResourceDictionary(new Uri(source));
            }
            if (value is Uri sourceUri)
            {
                return new ThemeResourceDictionary(sourceUri);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var source = value as ThemeResourceDictionary;
            if (source == null) return null;

            if (destinationType == typeof(ResourceDictionary))
            {
                return new ResourceDictionary();
            }
            if (destinationType == typeof(string))
            {
                return source.Source.ToString();
            }
            if (destinationType == typeof(Uri))
            {
                return source.Source;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}