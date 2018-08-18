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
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Communications;
using NullGuard;

namespace Eyedrivomatic.Device.Services
{
    public interface IDeviceService : IDisposable
    {
        Task InitializeAsync();

        ConnectionState ConnectionState { get; }

        [AllowNull]
        IDevice ConnectedDevice { get; }
        IList<DeviceDescriptor> AvailableDevices { get; }

        event EventHandler<ConnectedDeviceChangedArgs> ConnectedDeviceChanged;
        event EventHandler<ConnectionState> ConnectionStateChanged;

        Task ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken none);
        Task AutoConnectAsync(bool autoUpdateFirmware, CancellationToken none);
    }

    public class ConnectedDeviceChangedArgs : EventArgs
    {
        public ConnectedDeviceChangedArgs([AllowNull]IDevice prevDevice, [AllowNull]IDevice newDevice)
        {
            PrevDevice = prevDevice;
            NewDevice = newDevice;
        }

        [AllowNull]
        public IDevice PrevDevice { get; }

        [AllowNull]
        public IDevice NewDevice { get; }
    }
}
