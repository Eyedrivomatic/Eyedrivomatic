using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using System.Threading.Tasks;
using Prism.Logging;
using System.Diagnostics.Contracts;
using System;
using Prism.Mvvm;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IHardwareService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HardwareService : BindableBase, IHardwareService
    {
        private IServiceLocator ServiceLocator { get; }
        private ILoggerFacade Logger { get; }
      
        public HardwareService(IServiceLocator serviceLocator, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(serviceLocator != null, nameof(serviceLocator));

            AvailableDrivers = new ObservableCollection<IDriver>();

            ServiceLocator = serviceLocator;
            Logger = logger;
        }

        public ObservableCollection<IDriver> AvailableDrivers { get; }

        private IDriver _currentDriver;
        public IDriver CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        public Task InitializeAsync()
        {
            Logger?.Log("Initializing the Hardware Service.", Category.Debug, Priority.None);

            //TODO: Find the drivers that are actually plugged in.
            // And don't clear the array every time. Just add/remove as necessary
            AvailableDrivers.Clear();
            AvailableDrivers.AddRange(ServiceLocator.GetAllInstances<IDriver>());
            CurrentDriver = AvailableDrivers.FirstOrDefault();
            return Task.FromResult(true);
        }

    }
}
