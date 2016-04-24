using System;
using System.Diagnostics.Contracts;

using Prism.Logging;
using Prism.Modularity;


namespace Eyedrivomatic.Hardware
{
    public class HardwareModule : IModule
    {
        private ILoggerFacade Logger { get; }
        private IServiceProvider ServiceProvider { get; }

        public HardwareModule(IServiceProvider serviceProvider, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(serviceProvider != null, nameof(serviceProvider));

            ServiceProvider = serviceProvider;
            Logger = logger;
        }

        public void Initialize()
        {
            Logger?.Log("Initializing HardwareModule.", Category.Info, Priority.None);

            var hardwareService = (IHardwareService)ServiceProvider.GetService(typeof(IHardwareService));
            var task = hardwareService.InitializeAsync();
        }
    }
}
