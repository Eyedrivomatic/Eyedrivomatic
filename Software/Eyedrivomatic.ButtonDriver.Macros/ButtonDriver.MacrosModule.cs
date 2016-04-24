using System;
using System.Diagnostics.Contracts;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.Hardware;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Modules.Configuration.Views;

namespace Eyedrivomatic.Modules.Configuration
{
    public class ConfigurationModule : IModule
    {
        private readonly IHardwareService _hardwareService;
        public readonly IRegionManager RegionManager;

        public ConfigurationModule(IRegionManager regionManager, IHardwareService hardwareService)
        {
            Contract.Requires<ArgumentNullException>(regionManager != null, nameof(regionManager));
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));

            RegionManager = regionManager;
            _hardwareService = hardwareService;
        }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(ConfigurationView));
        }
    }
}
