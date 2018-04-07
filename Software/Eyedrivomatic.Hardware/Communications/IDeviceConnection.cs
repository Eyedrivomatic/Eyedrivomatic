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
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Services;

namespace Eyedrivomatic.Hardware.Communications
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public delegate void MessageReceivedHandler(string message);

    public interface IDeviceConnection : IDisposable
    {
        /// <summary>
        /// The connection string that was used to start the connection.
        /// This is the auto-detected configuration is none was supplied to the Connect method.
        /// </summary>
        string ConnectionString { get; }

        DeviceDescriptor Device { get; }

        /// <summary>
        /// The version and variant of the firmware running on the device.
        /// </summary>
        VersionInfo VersionInfo { get; }

        /// <summary>
        /// The current connection state. 
        /// After the connection is created or after a call to <see cref="Disconnect"/>, the state is <see cref="ConnectionState.Disconnected"/>"
        /// During the call to ConnectAsync the State is <see cref="ConnectionState.Connecting"/>
        /// After the connection has been established the state is <see cref="ConnectionState.Connected"/>
        /// If the connection failed, the state is <see cref="ConnectionState.Error"/>
        /// </summary>
        ConnectionState State { get; }

        /// <summary>
        /// Hot data stream. Each item is the data for a connected session. The data itself is represented as an observable stream of chars.
        /// </summary>
        IObservable<IObservable<char>> DataStream { get; }

        /// <summary>
        /// Fired when the device updates its status.
        /// All status values are refreshed befor this event fires.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Connect to the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ConnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send the raw message to the device.
        /// </summary>
        void SendMessage(string message);
    }
}