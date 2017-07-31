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

using Eyedrivomatic.Infrastructure;
using Microsoft.Practices.ServiceLocation;

namespace Eyedrivomatic.Controls
{
    /// <summary>
    /// The purpose of this module is to register custom controls and their dependencies.
    /// </summary>
    [ModuleExport(typeof(ControlsModule), InitializationMode = InitializationMode.WhenAvailable)]
    public class ControlsModule : IModule
    {
        public IServiceLocator ServiceLocator { get; set; }

        [ImportingConstructor]
        public ControlsModule(IServiceLocator serviceLocator)
        {
            Log.Debug(this, $"Creating Module {nameof(ControlsModule)}.");

            ServiceLocator = serviceLocator;
        }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(ControlsModule)}.");
        }
    }
}
