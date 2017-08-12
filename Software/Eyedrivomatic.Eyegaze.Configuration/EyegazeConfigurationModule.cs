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
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Eyegaze.Configuration.Views;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.Eyegaze.Configuration
{
    /// <summary>
    /// The purpose of this module is to register custom controls and their dependencies.
    /// </summary>
    [ModuleExport(typeof(EyegazeConfigurationModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames =  new[] { nameof(InfrastructureModule), nameof(ControlsModule), nameof(ConfigurationModule), nameof(EyegazeModule) })]
    public class EyegazeConfigurationModule : IModule, IDisposable
    {
        [ImportingConstructor]
        public EyegazeConfigurationModule()
        {
            Log.Debug(this, $"Creating Module {nameof(EyegazeConfigurationModule)}.");
        }

        [Import]
        public IRegionManager RegionManager { get; set; }

        [Import]
        private IDwellClickConfigurationService DwellClickConfigurationService { get; set; }

        [Import]
        private RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(EyegazeConfigurationModule)}.");

            RegionManager.RegisterViewWithRegion(RegionNames.SleepButtonRegion, typeof(SleepButton));
            RegisterConfigurationViews();

            DwellClickBehavior.DefaultConfiguration = DwellClickConfigurationService;
        }

        private void RegisterConfigurationViews()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(EyegazeConfigurationView));
            RegionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, 
                () => RegionNavigationButtonFactory.Create(
                    Resources.Strings.ViewName_EyegazeConfig,
                    RegionNames.ConfigurationContentRegion,
                    new Uri($@"/{nameof(EyegazeConfigurationView)}", UriKind.Relative),
                    1));
        }

        public void Dispose()
        {
        }
    }
}
