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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Accord.Video.DirectShow;
using Eyedrivomatic.Camera.Views;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Logging;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.Camera
{
    [ModuleExport(typeof(CameraModule),
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(CommonUiModule) })]
    public class CameraModule : IModule
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public CameraModule(IRegionManager regionManager)
        {
            Log.Info(this, $"Creating Module {nameof(CameraModule)}.");
            _regionManager = regionManager;
        }

        [Import]
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        [Export("GetCameras")]
        Func<IEnumerable<FilterInfo>> GetCameras => () => new FilterInfoCollection(FilterCategory.VideoInputDevice).Cast<FilterInfo>();

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(CameraModule)}.");

            _regionManager.RegisterViewWithRegion(RegionNames.ForwardViewCameraRegion, typeof(CameraView));

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(CameraConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () => RegionNavigationButtonFactory.Create(
                Translate.TranslationFor(nameof(Resources.Strings.ViewName_CameraConfiguration)),
                RegionNames.ConfigurationContentRegion,
                new Uri($@"/{nameof(CameraConfigurationView)}", UriKind.Relative), 3));

        }
    }
}
