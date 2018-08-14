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
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows.Input;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Device.Configuration.Services;
using Eyedrivomatic.Device.Configuration.Views;
using Eyedrivomatic.Device.Services;
using Prism.Mef.Modularity;
using Prism.Modularity;

using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Prism.Interactivity.InteractionRequest;
using Prism.Regions;

namespace Eyedrivomatic.Device.Configuration
{
    [ModuleExport(typeof(DeviceConfigurationModule), InitializationMode = InitializationMode.WhenAvailable)]
    public class DeviceConfigurationModule : IModule
    {
        [ImportingConstructor]
        public DeviceConfigurationModule()
        {
            Log.Debug(this, $"Creating Module {nameof(DeviceConfigurationModule)}.");
        }

        private readonly IDeviceService _deviceService;

        private readonly IDeviceConfigurationService _configurationService;
        private readonly IRegionManager _regionManager;
        private readonly InteractionRequest<INotification> _connectionFailureNotification;

        [Import]
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        [Import(nameof(ShowDisclaimerCommand))]
        public ICommand ShowDisclaimerCommand { get; set; }

        [ImportingConstructor]
        public DeviceConfigurationModule(IRegionManager regionManager, IDeviceService deviceService, 
            IDeviceConfigurationService configurationService, InteractionRequest<INotification> connectionFailureNotification)
        {
            Log.Debug(this, $"Creating Module {nameof(DeviceConfigurationModule)}.");

            _regionManager = regionManager;
            _deviceService = deviceService;
            _configurationService = configurationService;
            _connectionFailureNotification = connectionFailureNotification;
        }

        public async void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(DeviceConfigurationModule)}.");

            RegisterConfigurationViews();

            ShowDisclaimerCommand.Execute(null);

            try
            {
                await _deviceService.InitializeAsync();
                Log.Debug(this, $"ButtonDriverService Initialized. AutoConnect: [{_configurationService.AutoConnect}]");

                if (_deviceService.ConnectedDevice == null)
                {
                    Log.Error(this, "Failed to initialize hardware. No device selected.");
                    NavigateToConfiguration();
                    return;
                }

                if (_configurationService.AutoConnect)
                {
                    var connectionString = _configurationService.ConnectionString;
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        Log.Info(this, $"Connection string: [{connectionString}]");
                        await _deviceService.ConnectAsync(connectionString, true, CancellationToken.None);
                    }
                    else
                    {
                        Log.Warn(this, "Connection string not specified. Attempting to auto-detect.");
                        await _deviceService.AutoConnectAsync(true, CancellationToken.None);
                        if (_deviceService.ConnectedDevice != null)
                        {
                            //save the connection string for faster connection next time.
                            _configurationService.ConnectionString =
                                _deviceService.ConnectedDevice.Connection.ConnectionString;
                            _configurationService.Save();
                        }
                    }
                }
            }
            catch (ConnectionFailedException cfe)
            {
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = cfe.Message
                    });
                NavigateToConfiguration();
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Firmware version check failed! [{ex}]");
                _connectionFailureNotification.Raise(
                    new Notification
                    {
                        Title = Strings.DeviceConnection_Error,
                        Content = string.Format(Strings.DeviceConnection_Error_FirmwareCheck, _configurationService.ConnectionString)
                    });
                NavigateToConfiguration();
            }
        }

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(DeviceConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
                RegionNavigationButtonFactory.Create(
                    Translate.TranslationFor(nameof(Strings.ViewName_DeviceConfig)),
                    RegionNames.ConfigurationContentRegion,
                    new Uri($@"/{nameof(DeviceConfigurationView)}", UriKind.Relative),
                    3));
        }

        private void NavigateToConfiguration()
        {
            Log.Debug(this, $@"Navigating to [/{nameof(DeviceConfigurationView)}].");
            _regionManager.RequestNavigate(RegionNames.ConfigurationContentRegion, $"/{nameof(DeviceConfigurationView)}");
        }

        public void Dispose()
        {
            _deviceService?.Dispose();
        }
    }
}

