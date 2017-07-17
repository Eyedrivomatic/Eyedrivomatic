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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

using Prism.Commands;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using NullGuard;

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
            _configurationService = configurationService;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;
            RefreshAvailableDeviceListCommand = new DelegateCommand(RefreshAvailableDeviceList, CanRefreshAvailableDeviceList);
            AutoDetectDeviceCommand = new DelegateCommand(AutoDetectDevice, CanAutoDetectDevice);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);

            RefreshAvailableDeviceList();
        }

        private void ConfigurationService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public ICommand RefreshAvailableDeviceListCommand { get; }

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }

        public bool Connecting => Driver.Connection.State == ConnectionState.Connecting;
        public bool Connected => Driver.Connection.State == ConnectionState.Connected;
        public bool Ready => Driver.HardwareReady;
        
        public class SerialDeviceInfo
        {
            public readonly string Name;
            public readonly string Port;

            public SerialDeviceInfo(string name, string port)
            {
                Name = name;
                Port = port;
            }

            public override string ToString()
            {
                return $"{Port} - {Name}";
            }
        };

        private IList<SerialDeviceInfo> _availableDevices;
        public IList<SerialDeviceInfo> AvailableDevices
        {
            get => _availableDevices;
            set => SetProperty(ref _availableDevices, value);
        }

        [AllowNull]
        public SerialDeviceInfo SelectedDevice
        {
            get => AvailableDevices.FirstOrDefault(device => device.Port == _configurationService.ConnectionString); 
            set => _configurationService.ConnectionString = (value?.Port ?? string.Empty);
        }

        public bool AutoConnect
        {
            get => _configurationService.AutoConnect;
            set => _configurationService.AutoConnect = value;
        }

        public void RefreshAvailableDeviceList()
        {

            AvailableDevices = (from dev in Driver?.Connection.GetAvailableDevices() orderby dev.Item2 select new SerialDeviceInfo(dev.Item1, dev.Item2)).ToList();

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        public bool CanRefreshAvailableDeviceList()
        {
            return true;
        }

        protected async void AutoDetectDevice()
        {
            RefreshAvailableDeviceList();
            await Driver.Connection.AutoConnectAsync();
            SelectedDevice = AvailableDevices.FirstOrDefault(device => device.Port == Driver.Connection.ConnectionString);
        }

        protected bool CanAutoDetectDevice() { return !Connected && !Connecting; }

        protected async void Connect()
        {
            if (string.IsNullOrWhiteSpace(SelectedDevice?.Port)) throw new InvalidOperationException("Unable to connect - no device selected.");

            await Driver.Connection.ConnectAsync(SelectedDevice.Port);
        }

        protected bool CanConnect()
        {
            return SelectedDevice != null && !Connected && !Connecting;
        }

        protected void Disconnect()
        {
            Driver.Connection.Disconnect();
        }

        protected bool CanDisconnect()
        {
            return Connected;
        }

        protected override void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverStateChanged(sender, e);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }
    }
}
