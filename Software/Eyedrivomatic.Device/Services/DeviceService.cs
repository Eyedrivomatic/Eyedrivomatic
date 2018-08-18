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
using NullGuard;
using Prism.Events;

namespace Eyedrivomatic.Device.Services
{
    [Export(typeof(IDeviceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceFactory _deviceFactory;
        private readonly IEventAggregator _eventAggregator;


        [ImportingConstructor]
        public DeviceService(IDeviceFactory deviceFactory, IEventAggregator eventAggregator)
        {
            _deviceFactory = deviceFactory;
            _eventAggregator = eventAggregator;
        }

        public IList<DeviceDescriptor> AvailableDevices => _deviceFactory.GetAvailableDevices();

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (_connectionState == value) return;
                _connectionState = value;
                ConnectionStateChanged?.Invoke(this, value);
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(_connectionState);
            }
        }

        private IDevice _connectedDevice;
        private ConnectionState _connectionState;

        public event EventHandler<ConnectedDeviceChangedArgs> ConnectedDeviceChanged;
        public event EventHandler<ConnectionState> ConnectionStateChanged; 

        public async Task ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            try
            {
                ConnectedDevice?.Connection.Disconnect();
                ConnectedDevice = null;
                ConnectionState = ConnectionState.Connecting;
                ConnectedDevice = await _deviceFactory.ConnectAsync(connectionString, autoUpdateFirmware, cancellationToken);
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
                ConnectedDevice?.Connection.Disconnect();
                ConnectedDevice = null;
                ConnectionState = ConnectionState.Connecting;
                ConnectedDevice = await _deviceFactory.AutoConnectAsync(autoUpdateFirmware, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to connect [{ex}].");
                ConnectionState = ConnectionState.Error;
                throw;
            }
        }

        [AllowNull]
        public IDevice ConnectedDevice
        {
            get => _connectedDevice;
            private set
            {
                if (ReferenceEquals(_connectedDevice, value)) return;

                var currentDevice = _connectedDevice;
                if (currentDevice != null)
                {
                    (currentDevice as IDisposable)?.Dispose();
                    currentDevice.Connection.ConnectionStateChanged -= ConnectionOnConnectionStateChanged;
                }
                _connectedDevice = value;
                ConnectedDeviceChanged?.Invoke(this, new ConnectedDeviceChangedArgs(currentDevice, _connectedDevice));

                if (_connectedDevice != null)
                {
                    _connectedDevice.Connection.ConnectionStateChanged += ConnectionOnConnectionStateChanged;
                    ConnectionState = _connectedDevice.Connection.State;
                }
                else
                {
                    ConnectionState = ConnectionState.Disconnected;
                }
            }
        }

        private void ConnectionOnConnectionStateChanged(object sender, EventArgs eventArgs)
        {

            if (ConnectedDevice != null && ConnectedDevice.Connection.State == ConnectionState.Disconnected)
            {
                ConnectedDevice = null;
                return;
            }

            ConnectionState = ConnectedDevice?.Connection.State ?? ConnectionState.Disconnected;
        }

        public Task InitializeAsync()
        {
            Log.Debug(this, "Device Initialization Service Initializing.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            ConnectedDevice = null;
            ConnectionState = ConnectionState.Disconnected;
        }
    }
}
