﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class ValueCompareToBoolConverter : IValueConverter, IMultiValueConverter
    {
        public IComparer Comparer { get; set; } = new Comparer(CultureInfo.CurrentCulture);

        public object Convert([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return value == null && parameter == null;
            var typeConverter = new TypeConverter();

            if (!typeConverter.CanConvertTo(value.GetType())) return false;
            var compareTo = new TypeConverter().ConvertTo(parameter, value.GetType());
            return Comparer?.Compare(value, compareTo) == 0;
        }

        [return: AllowNull]
        public object ConvertBack([AllowNull] object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value as bool? ?? false) return parameter;
            return null;
        }

        public object Convert([AllowNull] object[] values, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (values == null || values.Length <= 1) return false;
            var first = values.First();
            return values.Skip(1).All(v => Comparer.Compare(first, v) == 0);
        }

        [return: AllowNull]
        public object[] ConvertBack([AllowNull] object value, Type[] targetTypes, [AllowNull] object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value) return new object[] { parameter?.ToString() };
            return null;
        }
    }
}