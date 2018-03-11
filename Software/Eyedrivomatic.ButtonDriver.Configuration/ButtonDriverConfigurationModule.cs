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


using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [ModuleExport(typeof(ButtonDriverConfigurationModule), InitializationMode = InitializationMode.WhenAvailable)]
    public class ButtonDriverConfigurationModule : IModule
    {
        [ImportingConstructor]
        public ButtonDriverConfigurationModule()
        {
            Log.Debug(this, $"Creating Module {nameof(ButtonDriverConfigurationModule)}.");
        }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(ButtonDriverConfigurationModule)}.");
        }
    }
}
