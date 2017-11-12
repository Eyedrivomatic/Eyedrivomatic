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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.ButtonDriver.Macros
{
    [ModuleExport(typeof(MacrosModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(ButtonDriverHardwareModule), nameof(InfrastructureModule) })]
    public class MacrosModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly IMacroSerializationService _serializationService;

        [Export("DrivingPageMacro")]
        public IMacro DrivingPageMacro => _serializationService.LoadMacros().FirstOrDefault();

        [ImportingConstructor]
        public MacrosModule(IRegionManager regionManager, IHardwareService hardwareService, IMacroSerializationService serializationService)
        {
            Log.Info(this, $"Creating Module {nameof(MacrosModule)}.");

            _regionManager = regionManager;
            _serializationService = serializationService;
        }

        public void Initialize()
        {
            Log.Info(this, $"Initializing Module {nameof(MacrosModule)}.");

            SetSerializationPath();

            //_regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(EditMacrosView));
        }

        private void SetSerializationPath()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var appPath = Path.GetDirectoryName(uri.LocalPath);
            _serializationService.ConfigurationFilePath = Path.Combine(appPath, "Macros.config");
        }
    }
}
