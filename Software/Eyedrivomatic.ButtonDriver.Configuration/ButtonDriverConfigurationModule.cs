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
