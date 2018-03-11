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


using System;
using System.ComponentModel.Composition;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Eyegaze.Configuration.Views;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
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
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

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
                    Translate.TranslationFor(nameof(Resources.Strings.ViewName_EyegazeConfig)),
                    RegionNames.ConfigurationContentRegion,
                    new Uri($@"/{nameof(EyegazeConfigurationView)}", UriKind.Relative),
                    1));
        }

        public void Dispose()
        {
        }
    }
}
