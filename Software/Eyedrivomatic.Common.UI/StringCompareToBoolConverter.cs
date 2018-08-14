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
using System.Linq;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Common.UI
{
    public class StringCompareToBoolConverter : IValueConverter, IMultiValueConverter
    {
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;

        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            var compareTo = parameter?.ToString();
            var strValue = value?.ToString();
            return string.Compare(strValue, compareTo, StringComparison) == 0;
        }

        [return: AllowNull]
        public object ConvertBack([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value) return parameter ;
            return null;
        }

        public object Convert([AllowNull] object[] values, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (values == null || values.Length <= 1) return false;
            var first = values.First()?.ToString();
            return values.Skip(1).All(v => string.Compare(first, v?.ToString(), StringComparison) == 0);
        }

        [return: AllowNull]
        public object[] ConvertBack([AllowNull] object value, Type[] targetTypes, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value is bool && (bool) value) return new [] { parameter };
            return null;
        }
    }
}
