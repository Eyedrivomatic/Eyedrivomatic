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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    [Serializable]
    public class ValueToImageConverter :  Dictionary<Enum, Image>, IValueConverter
    {
        public ValueToImageConverter()
        { }

        protected ValueToImageConverter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ImageIfNone = info.GetValue(nameof(ImageIfNone), typeof(Image)) as Image;
            ImageSourceIfNone = info.GetValue(nameof(ImageSourceIfNone), typeof(ImageSource)) as ImageSource;
        }

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

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ImageIfNone), ImageIfNone);
            info.AddValue(nameof(ImageSourceIfNone), ImageSourceIfNone);
        }
    }
}