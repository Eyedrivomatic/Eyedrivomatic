// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    [Serializable]
    public class ValueToImageConverter<T> : Dictionary<T, ImageSource>, IValueConverter
    {
        protected ValueToImageConverter() { }

        protected ValueToImageConverter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //Recomended by CA2240
            base.GetObjectData(info, context);
        }

        [AllowNull] public ImageSource ImageIfNone { get; set; }

        [return: AllowNull]
        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value == null) return ImageIfNone;

            var val = (T)System.Convert.ChangeType(value, typeof(T));
            return !ContainsKey(val) ? ImageIfNone : this[val];
        }

        [return: AllowNull]
        public object ConvertBack([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
