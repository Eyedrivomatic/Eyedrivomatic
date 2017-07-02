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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [Export(typeof(IMacroSerializationService))]
    public class MacroSerializationService : IMacroSerializationService
    {
        public string ConfigurationFilePath { get; set; }

        public IEnumerable<IMacro> LoadMacros()
        {
            Log.Debug(this, $"Loading Macros from [{ConfigurationFilePath}].");

            using (var reader = new StreamReader(ConfigurationFilePath))
            {
                var serializer = new XmlSerializer(typeof(UserMacro[]), new XmlRootAttribute("Macros"));
                return serializer.Deserialize(reader) as UserMacro[];
            }
        }

        public void SaveMacros(IEnumerable<IMacro> macros)
        {
            Log.Debug(this, $"Saving Macros to [{ConfigurationFilePath}].");

            var serializer = new XmlSerializer(typeof(UserMacro[]), new XmlRootAttribute("Macros"));

            using (var writer = new StreamWriter(ConfigurationFilePath))
            {
                serializer.Serialize(writer, macros.OfType<UserMacro>().ToArray());
            }
        }
    }
}
