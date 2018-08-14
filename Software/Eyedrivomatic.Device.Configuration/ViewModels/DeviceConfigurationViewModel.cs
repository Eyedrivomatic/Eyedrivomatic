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
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Configuration.Services;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;

using NullGuard;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.Device.Configuration.ViewModels
{
    public abstract class DeviceConfigurationViewModel : DeviceViewModelBase, IHeaderInfoProvider<string>
    {
        private readonly IDeviceConfigurationService _configurationService;
        private readonly IDisposable _saveCommandRegistration;
        private readonly InteractionRequest<INotification> _connectionFailureNotification;

        public DeviceConfigurationViewModel(
            IDeviceService deviceService, 
            IDeviceConfigurationService configurationService,
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand,
            InteractionRequest<INotification> connectionFailureNotification)
            : base(deviceService)
        {
            _configurationService = configurationService;
            _connectionFailureNotification = connectionFailureNotification;
            _configurationService.PropertyChanged += ConfigurationService_PropertyChanged;

            AutoDetectDeviceCommand = new DelegateCommand(AutoDetectDevice, CanAutoDetectDevice);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);

            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }

        public string HeaderInfo => Strings.ViewName_DeviceConfig;

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }

        public bool Connecting => DeviceService.ConnectionState == ConnectionState.Connecting; //Only the device service really knows about connecting.
        public bool Connected => DeviceService.ConnectionState == ConnectionState.Connected;
        public bool Ready => Device?.DeviceReady ?? false;

        public IList<DeviceDescriptor> AvailableDevices => DeviceService.AvailableDevices;

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

        protected async void AutoDetectDevice()
        {
            try
            {
                await DeviceService.AutoConnectAsync(true, CancellationToken.None);
                SelectedDevice = AvailableDevices.FirstOrDefault(device => device.ConnectionString == Device.Connection?.ConnectionString);
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
                await DeviceService.ConnectAsync(SelectedDevice.ConnectionString, true, CancellationToken.None);
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
            Device.Connection.Disconnect();
        }

        protected bool CanDisconnect()
        {
            return Connected;
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
        protected override void OnDeviceStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDeviceStateChanged(sender, e);

            if (e.PropertyName == nameof(Device.DeviceReady)) RaisePropertyChanged(nameof(Ready));

            if (e.PropertyName == nameof(Device.Connection))
            {
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                AutoDetectDeviceCommand.RaiseCanExecuteChanged();

                RaisePropertyChanged(nameof(Connecting));
                RaisePropertyChanged(nameof(Connected));
            }
        }

        private static readonly Dictionary<string, string[]> ConfigurationPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IDeviceConfigurationService.HasChanges), new [] {nameof(HasChanges)} },
            { nameof(IDeviceConfigurationService.AutoConnect), new [] {nameof(AutoConnect)} },
            { nameof(IDeviceConfigurationService.ConnectionString), new [] {nameof(SelectedDevice)} },
        };

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverSettingsChanged(sender, e);
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
                await Device.DeviceSettings.Save();
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
