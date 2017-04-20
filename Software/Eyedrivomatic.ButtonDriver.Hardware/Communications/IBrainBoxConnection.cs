using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Prism.Events;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public class ConnectionStateEvent : PubSubEvent<ConnectionState>{}

    public delegate void MessageReceivedHandler(string message);

    [ContractClass(typeof(Contracts.BranBoxConnectionContract))]
    public interface IBrainBoxConnection : IDisposable
    {
        /// <summary>
        /// The connection string that was used to start the connection.
        /// This is the auto-detected configuration is none was supplied to the Connect method.
        /// </summary>
        string ConnectionString { get; }

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
        /// Get a list of potential devices that are connected to the computer.
        /// The first part is the device friendly name, the second part is the port.
        /// For the Arduino platform, this is a list of all COM ports.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Tuple<string, string>> GetAvailableDevices();

        /// <summary>
        /// Attempt to automatically detect the device.
        /// </summary>
        Task AutoConnectAsync();

        /// <summary>
        /// Connect to the device.
        /// For the current Arduino platform, the configuration "COM#" where # is the COM port number
        /// assigned to the device.
        /// </summary>
        /// <param name="connectionString">The platform specific connection string.</param>
        Task ConnectAsync(string connectionString);

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send the raw message to the device.
        /// </summary>
        void SendMessage(string message);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IBrainBoxConnection))]
        internal abstract class BranBoxConnectionContract : IBrainBoxConnection
        {
            public abstract string ConnectionString { get; }
            public abstract ConnectionState State { get; }

            public IObservable<IObservable<char>> DataStream
            {
                get
                {
                    Contract.Ensures(Contract.Result<IObservable<IObservable<char>>>() != null);
                    throw new NotImplementedException();
                }
            }

            public abstract event EventHandler ConnectionStateChanged;

            public IEnumerable<Tuple<string, string>> GetAvailableDevices()
            {
                throw new NotImplementedException();
            }

            public Task AutoConnectAsync()
            {
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
            }

            public Task ConnectAsync(string connectionString)
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(connectionString), nameof(connectionString));
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
            }

            public abstract void Disconnect();
            public abstract void Dispose();
            public void SendMessage(string message)
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(message), nameof(message));
            }
        }
    }
}