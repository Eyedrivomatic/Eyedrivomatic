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
using System.Resources;
using System.Windows.Data;

using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class LocalizedEnumNameConverter : IValueConverter
    {
        public string ResourcePrefix { get; set; }
        public Type EnumType { get; set; }
        public ResourceManager ResourceManager { get; set; }

        public object Convert(object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (EnumType == null) throw new ApplicationException("enum type not specified.");

            var valueName = Enum.GetName(EnumType, value);
            var resourceName = $"{ResourcePrefix}{EnumType.Name}_{valueName}";
            var result = ResourceManager?.GetString(resourceName);
            if (string.IsNullOrWhiteSpace(result)) return valueName;
            return result;
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
