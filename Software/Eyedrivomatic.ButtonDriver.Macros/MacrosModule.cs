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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;

using Prism.Logging;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.ButtonDriver.Macros.Views;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.Macros
{
    [ModuleExport(typeof(MacrosModule), DependsOnModuleNames = new[] { nameof(ButtonDriverHardwareModule), nameof(InfrastructureModule) }, InitializationMode = InitializationMode.WhenAvailable)]
    public class MacrosModule : IModule
    {
        private readonly IHardwareService _hardwareService;
        private readonly IRegionManager _regionManager;
        private readonly IMacroSerializationService _serializationService;

        [Import]
        public static ILoggerFacade Logger { get; set; }

        [Export("DrivingPageMacro")]
        public IMacro DrivingPageMacro => _serializationService.LoadMacros().FirstOrDefault();

        [ImportingConstructor]
        public MacrosModule(IRegionManager regionManager, IHardwareService hardwareService, IMacroSerializationService serializationService)
        {
            Contract.Requires<ArgumentNullException>(regionManager != null, nameof(regionManager));
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(serializationService != null, nameof(serializationService));

            Logger?.Log($"Creating Module {nameof(MacrosModule)}.", Category.Info, Priority.None);

            _regionManager = regionManager;
            _hardwareService = hardwareService;
            _serializationService = serializationService;
        }

        public void Initialize()
        {
            Logger?.Log($"Initializing Module {nameof(MacrosModule)}.", Category.Info, Priority.None);

            SetSerializationPath();

            //_regionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(EditMacrosView));
            _regionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(ExecuteMacrosView));
        }

        private void SetSerializationPath()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var appPath = Path.GetDirectoryName(uri.LocalPath);
            _serializationService.ConfigurationFilePath = Path.Combine(appPath, "Macros.config");
        }
    }
}
