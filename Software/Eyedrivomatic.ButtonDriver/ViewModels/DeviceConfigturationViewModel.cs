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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Input;

using Prism.Commands;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Device;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.ButtonDriver.Device.Models;
using Eyedrivomatic.ButtonDriver.Device.Services;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class DeviceConfigturationViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly IDisposable _saveCommandRegistration;
        private readonly InteractionRequest<INotification> _connectionFailureNotification;

        [ImportingConstructor]
        public DeviceConfigturationViewModel(
            IDeviceInitializationService deviceInitializationService, 
            IButtonDriverConfigurationService configurationService,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand,
            InteractionRequest<INotification> connectionFailureNotification)
            : base(deviceInitializationService)
        {
            _configurationService = configurationService;
            _connectionFailureNotification = connectionFailureNotification;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;

            RefreshAvailableDeviceListCommand = new DelegateCommand(RefreshAvailableDeviceList, CanRefreshAvailableDeviceList);
            AutoDetectDeviceCommand = new DelegateCommand(AutoDetectDevice, CanAutoDetectDevice);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);
            TrimCommand = new DelegateCommand<Direction?>(Trim, CanTrim)
                .ObservesProperty(() => TrimPosition)
                .ObservesProperty(() => MaxSpeed);

            RefreshAvailableDeviceList();

            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public ICommand RefreshAvailableDeviceListCommand { get; }

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand<Direction?> TrimCommand { get; } 

        public bool Connecting => Driver?.Connection?.State == ConnectionState.Connecting;
        public bool Connected => Driver?.Connection?.State == ConnectionState.Connected;
        public bool Ready => Driver.DeviceReady;
        
        private IList<DeviceDescriptor> _availableDevices;
        public IList<DeviceDescriptor> AvailableDevices
        {
            get => _availableDevices;
            set => SetProperty(ref _availableDevices, value);
        }

        [AllowNull]
        public DeviceDescriptor SelectedDevice
        {
            get => AvailableDevices.FirstOrDefault(device => device.ConnectionString == _configurationService.ConnectionString); 
            set => _configurationService.ConnectionString = value?.ConnectionString ?? string.Empty;
        }

        public bool AutoConnect
        {
            get => _configurationService.AutoConnect;
            set => _configurationService.AutoConnect = value;
        }

        public void RefreshAvailableDeviceList()
        {
            AvailableDevices = Driver.GetAvailableDevices(true).ToList();

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
            try
            {
                RefreshAvailableDeviceList();
                await Driver.AutoConnectAsync(true, CancellationToken.None);
                SelectedDevice = AvailableDevices.FirstOrDefault(device => device.ConnectionString == Driver.Connection?.ConnectionString);

            }
            catch (ConnectionFailedException cfe)
            {
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = cfe.Message
                    });
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Unexpected exception while connecting to device! [{ex}]");
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = Strings.DeviceConnection_Error_Auto_NotFound
                    });
            }
        }

        protected bool CanAutoDetectDevice() { return !Connected && !Connecting; }

        protected async void Connect()
        {
            if (SelectedDevice == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(SelectedDevice?.ConnectionString)) throw new InvalidOperationException("Unable to connect - no device selected.");
                await Driver.ConnectAsync(SelectedDevice.ConnectionString, true, CancellationToken.None);
            }
            catch (ConnectionFailedException cfe)
            {
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = cfe.Message
                    });
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Firmware version check failed! [{ex}]");
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = string.Format(Strings.DeviceConnection_Error_FirmwareCheck, SelectedDevice?.ConnectionString ?? "N/A")
                    });
            }
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

        public Point TrimPosition => Driver?.DeviceSettings?.CenterPos ?? new Point(0,0);

        protected void Trim(Direction? direction)
        {
            if (!Connected || direction == null || Driver.DeviceSettings?.CenterPos == null) return;

            var centerPos = Driver.DeviceSettings.CenterPos.Value;

            var actionDictionary = new Dictionary<Direction, Action>
            {
                { Direction.Forward, () => centerPos.Y++ },
                { Direction.Backward, () => centerPos.Y-- },
                { Direction.Right, () => centerPos.X++ },
                { Direction.Left, () => centerPos.X-- }
            };
            if (!actionDictionary.ContainsKey(direction.Value)) return;
            actionDictionary[direction.Value]();

            Driver.DeviceSettings.CenterPos = centerPos;
        }

        protected bool CanTrim(Direction? direction)
        {
            if (!Connected || direction == null) return false;

            var testDictionary = new Dictionary<Direction, Func<bool>>
            {
                { Direction.Forward, () => Driver.DeviceSettings.MaxSpeed > 0 },
                { Direction.Backward, () => Driver.DeviceSettings.MaxSpeed > 0 },
                { Direction.Right, () => Driver.DeviceSettings.MaxSpeed > 0 },
                { Direction.Left, () => Driver.DeviceSettings.MaxSpeed > 0 }
            };

            if (!testDictionary.ContainsKey(direction.Value)) return false;
            return testDictionary[direction.Value]();
        }

        public decimal MaxSpeed
        {
            get => Driver?.DeviceSettings?.MaxSpeed ?? 0;
            set => Driver.DeviceSettings.MaxSpeed = value;
        }
        public decimal DeviceSpeedLimit => Driver.DeviceSettings.DeviceMaxSpeed;

        public DeviceOrientation Orientation
        {
            get => Driver?.DeviceSettings?.Orientation ?? DeviceOrientation.Rotate0Deg;
            set => Driver.DeviceSettings.Orientation = value;
        }

        private bool _deviceHasChanges;

        public bool HasChanges
        {
            get => _configurationService.HasChanges || _deviceHasChanges;
            private set
            {
                _deviceHasChanges = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SaveCommand => new DelegateCommand(Save).ObservesCanExecute(() => HasChanges);

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverStateChanged(sender, e);

            if (e.PropertyName == nameof(Driver.DeviceReady)) RaisePropertyChanged(nameof(Ready));

            if (e.PropertyName == nameof(Driver.Connection))
            {
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                AutoDetectDeviceCommand.RaiseCanExecuteChanged();
                TrimCommand.RaiseCanExecuteChanged();

                RaisePropertyChanged(nameof(Connecting));
                RaisePropertyChanged(nameof(Connected));
            }
        }

        private static readonly Dictionary<string, string[]> SettingPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IDeviceSettings.CenterPos),  new [] {nameof(TrimPosition), nameof(MaxSpeed)} },
            { nameof(IDeviceSettings.MaxSpeed), new []{ nameof(MaxSpeed)} },
            { nameof(IDeviceSettings.Orientation), new []{ nameof(Orientation)} },
        };

        private static readonly Dictionary<string, string[]> ConfigurationPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IButtonDriverConfigurationService.HasChanges), new [] {nameof(HasChanges)} },
            { nameof(IButtonDriverConfigurationService.AutoConnect), new [] {nameof(AutoConnect)} },
            { nameof(IButtonDriverConfigurationService.ConnectionString), new [] {nameof(SelectedDevice)} },
        };

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!SettingPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in SettingPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }
            HasChanges = true;
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private void ConfigurationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ConfigurationPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in ConfigurationPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        private async void Save()
        {
            try
            {
                _configurationService.Save();
                await Driver.DeviceSettings.Save();
                HasChanges = false;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to save device settings - [{ex}].");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _saveCommandRegistration?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
