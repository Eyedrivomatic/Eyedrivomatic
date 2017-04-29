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
using System.Globalization;
using System.Windows.Data;

using Eyedrivomatic.Resources;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class LocalizedEnumNameConverter : IValueConverter
    {
        public string ResourcePrefix { get; set; }
        public Type EnumType { get; set; }

        public object Convert(object value, Type targetType, [AllowNull] object parameter, CultureInfo culture)
        {
            if (EnumType == null) throw new ApplicationException("enum type not specified.");

            var valueName = Enum.GetName(EnumType, value);
            var resourceName = $"{ResourcePrefix}{EnumType.Name}_{valueName}";
            var result = Strings.ResourceManager.GetString(resourceName);
            if (string.IsNullOrWhiteSpace(result)) return valueName;
            return result;
        }

        public object ConvertBack(object value, Type targetType, [AllowNull]  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
