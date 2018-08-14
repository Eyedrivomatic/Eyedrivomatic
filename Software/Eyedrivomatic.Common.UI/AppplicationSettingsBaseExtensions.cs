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


using System.Configuration;
using System.Linq;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Common.UI
{
    public static class AppplicationSettingsBaseExtensions
    {
        public static void WriteToLog(this ApplicationSettingsBase settings)
        {
            Log.Debug(settings, $"Loaded {settings.SettingsKey} configuration.");
            foreach (var prop in settings.PropertyValues.Cast<SettingsPropertyValue>())
            {
                Log.Debug(settings, $"{prop.Name}: {prop.PropertyValue} {(prop.UsingDefaultValue ? " *" : "")}");
            }
        }

    }
}
