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


using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using System.Threading.Tasks;
using Prism.Logging;
using System.Diagnostics.Contracts;
using System;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IHardwareService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class HardwareService : IHardwareService
    {
        private IServiceLocator ServiceLocator { get; }
        private ILoggerFacade Logger { get; }

        [ImportingConstructor]
        public HardwareService(IServiceLocator serviceLocator, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(serviceLocator != null, nameof(serviceLocator));

            AvailableDrivers = new ObservableCollection<IButtonDriver>();

            ServiceLocator = serviceLocator;
            Logger = logger;
        }

        public ObservableCollection<IButtonDriver> AvailableDrivers { get; }

        private IButtonDriver _currentDriver;

        public event EventHandler CurrentDriverChanged;

        public IButtonDriver CurrentDriver
        {
            get { return _currentDriver; }
            set
            {
                _currentDriver = value;
                CurrentDriverChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task InitializeAsync()
        {
            Logger?.Log("Initializing the Hardware Service.", Category.Debug, Priority.None);

            //TODO: Find the drivers that are actually plugged in.
            // And don't clear the array every time. Just add/remove as necessary
            AvailableDrivers.Clear();
            AvailableDrivers.AddRange(ServiceLocator.GetAllInstances<IButtonDriver>());
            CurrentDriver = AvailableDrivers.FirstOrDefault();

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            CurrentDriver?.Dispose();
        }
    }
}
