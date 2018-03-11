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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Eyedrivomatic.Logging;

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
