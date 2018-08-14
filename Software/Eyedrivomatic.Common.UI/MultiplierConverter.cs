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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Common.UI
{
    public class MultiplierConverter : IValueConverter
    {
        public object Convert([AllowNull]object value, Type targetType, [AllowNull]object parameter, CultureInfo culture)
        {
            var converter = new DoubleConverter();
            var multiplier = parameter as int? ?? parameter as double? ?? (double) (converter.ConvertFrom(parameter) ?? 1d);
            var dVal = value as int? ?? value as double? ?? (double) (converter.ConvertFrom(value) ?? 0d);
            return multiplier * dVal;
        }

        public object ConvertBack([AllowNull]object value, Type targetType, [AllowNull]object parameter, CultureInfo culture)
        {
            var converter = new DoubleConverter();
            var multiplier = parameter as int? ?? parameter as double? ?? (double)(converter.ConvertFrom(parameter) ?? 1d);
            var dVal = value as int? ?? value as double? ?? (double)(converter.ConvertFrom(value) ?? 0d);
            return dVal /multiplier;
        }
    }
}
