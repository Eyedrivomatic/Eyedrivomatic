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
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using Prism.Events;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Infrastructure.Events;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class ArduinoConfigViewModel : DeviceConfigViewModel
    {
        [ImportingConstructor]
        public ArduinoConfigViewModel(IHardwareService hardwareService, IEventAggregator eventAggregator)
            : base(hardwareService, eventAggregator)
        {
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(eventAggregator != null, nameof(eventAggregator));

            _deviceConnectionString = hardwareService.CurrentDriver?.ConnectionString;
        }

        private string _deviceConnectionString;
        public override string SelectedDevice
        {
            get
            {
                return _deviceConnectionString;
            }
            set
            {
                SetProperty(ref _deviceConnectionString, value);

                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                AutoDetectDeviceCommand.RaiseCanExecuteChanged();
            }
        }

        protected override Task AutoDetectDeviceAsync()
        {
            return HardwareService.CurrentDriver?.AutoDetectDeviceAsync();
        }

        protected override void SaveChanges()
        {
            EventAggregator.GetEvent<SaveDeviceConnectionStringEvent>().Publish(_deviceConnectionString);

            base.SaveChanges();
        }

        protected override bool CanSaveChanges()
        {
            if (string.CompareOrdinal(_deviceConnectionString, HardwareService.CurrentDriver?.ConnectionString) != 0) return true;

            return base.CanSaveChanges();
        }

        protected override Task ConnectAsync()
        {
            return HardwareService.CurrentDriver?.ConnectAsync(_deviceConnectionString);
        }

        protected override bool CanConnect()
        {
            if (string.IsNullOrEmpty(SelectedDevice)) return false;
            return base.CanConnect();
        }

        protected override void Disconnect()
        {
            HardwareService.CurrentDriver?.Disconnect();
        }

        protected override void OnDriverStatusChanged(object sender, EventArgs e)
        {
            base.OnDriverStatusChanged(sender, e);

            if (Connected) SaveCommand.RaiseCanExecuteChanged();
        }

    }
}
