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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Device.Services
{
    [Export(typeof(IDeviceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DeviceService : IDeviceService
    {
        private readonly IConnectionEnumerationService _connectionEnumerationService;
        private readonly IDeviceFactory _deviceFactory;        


        [ImportingConstructor]
        public DeviceService(IConnectionEnumerationService connectionEnumerationService, IDeviceFactory deviceFactory)
        {
            _connectionEnumerationService = connectionEnumerationService;
            _deviceFactory = deviceFactory;
        }

        public IList<DeviceDescriptor> AvailableDevices => _connectionEnumerationService.GetAvailableDevices(true);

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (_connectionState == value) return;
                _connectionState = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
        }

        private IDevice _connectedDevice;
        private ConnectionState _connectionState;

        public event EventHandler ConnectedDeviceChanged;
        public event EventHandler<ConnectionState> ConnectionStateChanged; 

        public async Task ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            try
            {
                ConnectionState = ConnectionState.Connecting;
                ConnectedDevice = await _deviceFactory.ConnectAsync(connectionString, autoUpdateFirmware, cancellationToken);
                ConnectionState = ConnectedDevice?.Connection.State ?? ConnectionState.Disconnected;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to connect [{ex}].");
                ConnectionState = ConnectionState.Error;
                throw;
            }
        }

        public async Task AutoConnectAsync(bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            try
            {
                ConnectionState = ConnectionState.Connecting;
                ConnectedDevice = await _deviceFactory.AutoConnectAsync(autoUpdateFirmware, cancellationToken);
                ConnectionState = ConnectedDevice?.Connection.State ?? ConnectionState.Disconnected;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to connect [{ex}].");
                ConnectionState = ConnectionState.Error;
                throw;
            }
        }

        public IDevice ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                if (_connectedDevice != null)
                {
                    (_connectedDevice as IDisposable)?.Dispose();
                    _connectedDevice.Connection.ConnectionStateChanged -= ConnectionOnConnectionStateChanged;
                }
                _connectedDevice = value;

                if (_connectedDevice != null)
                {
                    _connectedDevice.Connection.ConnectionStateChanged += ConnectionOnConnectionStateChanged;
                }
                ConnectedDeviceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ConnectionOnConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            ConnectionStateChanged?.Invoke(this, ConnectionState);
        }

        public Task InitializeAsync()
        {
            Log.Debug(this, "Device Initialization Service Initializing.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            ConnectedDevice = null;
        }
    }
}
