using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    [Serializable]
    public class ValueToImageConverter : Dictionary<Enum, Image>, IValueConverter
    {
        [AllowNull]
        public ImageSource ImageSourceIfNone
        {
            get => ImageIfNone?.Source;
            set
            {
                if (value == null)
                {
                    ImageIfNone = null;
                    return;
                }

                if (ImageIfNone == null) ImageIfNone = new Image();
                ImageIfNone.Source = value;
            }
        }

        [AllowNull]
        public Image ImageIfNone { get; set; }

        [return: AllowNull]
        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value == null) return ImageIfNone;

            return !ContainsKey((Enum) value) ? ImageIfNone : this[(Enum)value];
        }

        [return: AllowNull]
        public object ConvertBack([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}