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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class DeviceConfigViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IButtonDriverConfigurationService _configurationService;

        [ImportingConstructor]
        public DeviceConfigViewModel(IHardwareService hardwareService, IButtonDriverConfigurationService configurationService)
            : base(hardwareService)
        {
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(configurationService != null, nameof(configurationService));

            _configurationService = configurationService;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;
            SaveCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
            RefreshAvailableDeviceListCommand = new DelegateCommand(RefreshAvailableDeviceList, CanRefreshAvailableDeviceList);
            AutoDetectDeviceCommand = DelegateCommand.FromAsyncHandler(AutoDetectDeviceAsync, CanAutoDetectDevice);
            ConnectCommand = DelegateCommand.FromAsyncHandler(ConnectAsync, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);
        }

        private void ConfigurationService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged();

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public ICommand RefreshAvailableDeviceListCommand { get; }

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public bool Connecting => HardwareService.CurrentDriver?.IsConnecting ?? false;
        public bool Connected => HardwareService.CurrentDriver?.IsConnected ?? false;
        public bool Ready => HardwareService.CurrentDriver?.HardwareReady ?? false;

        public IList<string> AvailableDevices => HardwareService.CurrentDriver?.GetAvailableDevices();

        public string SelectedDevice
        {
            get { return _configurationService.ConnectionString; }
            set { _configurationService.ConnectionString = value; }
        }

        public bool AutoConnect
        {
            get { return _configurationService.AutoConnect; }
            set { _configurationService.AutoConnect = value; }
        }

        public void RefreshAvailableDeviceList()
        {
            HardwareService.CurrentDriver?.GetAvailableDevices();
            OnPropertyChanged(nameof(AvailableDevices));

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        public bool CanRefreshAvailableDeviceList()
        {
            return true;
        }

        protected void SaveChanges()
        {
            _configurationService.Save();
            HardwareService.CurrentDriver?.SaveSettings();
            SaveCommand.RaiseCanExecuteChanged();
        }

        protected bool CanSaveChanges()
        {
            return (HardwareService.CurrentDriver?.IsConnected ?? false) || _configurationService.HasChanges;
        }

        protected async Task AutoDetectDeviceAsync()
        {
            SelectedDevice = await HardwareService.CurrentDriver?.AutoDetectDeviceAsync();
        }

        protected bool CanAutoDetectDevice() { return !Connected && !Connecting; }

        protected Task ConnectAsync()
        {
            return HardwareService.CurrentDriver?.ConnectAsync(SelectedDevice);
        }

        protected bool CanConnect()
        {
            return !string.IsNullOrEmpty(SelectedDevice) && !Connected && !Connecting;
        }

        protected void Disconnect()
        {
            HardwareService.CurrentDriver?.Disconnect();
        }

        protected bool CanDisconnect()
        {
            return Connected;
        }

        protected override void OnDriverStatusChanged(object sender, EventArgs e)
        {
            base.OnDriverStatusChanged(sender, e);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }
    }
}
