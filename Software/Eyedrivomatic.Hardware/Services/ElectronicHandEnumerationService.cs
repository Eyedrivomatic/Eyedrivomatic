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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Logging;
using NullGuard;
using Eyedrivomatic.Infrastructure.Extensions;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IDeviceEnumerationService))]
    public class ElectronicHandEnumerationService : IDeviceEnumerationService
    {
        private readonly IElectronicHandConnectionFactory _connectionFactory;
        private readonly IEnumerable<IElectronicHandDeviceInfo> _infos;

        [ImportingConstructor]
        public ElectronicHandEnumerationService(IElectronicHandConnectionFactory connectionFactory, [ImportMany] IEnumerable<IElectronicHandDeviceInfo> infos)
        {
            _connectionFactory = connectionFactory;
            _infos = infos;
        }

        [return: AllowNull]
        public async Task<IDeviceConnection> DetectDeviceAsync(CancellationToken cancellationToken)
        {
            var devices = GetAvailableDevices(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (!devices.Any()) return null;

            var foundCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, foundCts.Token);

            var connections = from device in devices
                select _connectionFactory.CreateConnection(device.ConnectionString);
            var connectionTasks = (from connection in connections
                select new {Connection = connection, Task = connection.ConnectAsync(cts.Token)}).ToList();
            
            while (connectionTasks.Any())
            {
                await Task.WhenAny(connectionTasks.Select(ct => ct.Task).Union(new[] {cts.Token.AsTask()}));
                cancellationToken.ThrowIfCancellationRequested();

                var connectionTask = connectionTasks.First(ct => ct.Task.IsCompleted);

                if (!connectionTask.Task.IsFaulted)
                {
                    Log.Info(this, $"Found device on [{connectionTask.Connection.ConnectionString}]!");
                    foundCts.Cancel();
                    return connectionTask.Connection;
                }

                connectionTasks.Remove(connectionTask);
            }

            return null;
        }

        public IList<DeviceDescriptor> GetAvailableDevices(bool includeAllSerialDevices)
        {
            if (!includeAllSerialDevices)
            {
                var filter = _infos.SelectMany(i => i.EyedrivomaticIds.Values).ToList();
                var devices = UsbSerialDeviceEnumerator.EnumerateDevices(filter)
                    .OfType<DeviceDescriptor>().ToList();

                if (devices.Any()) return devices;
            }

            return UsbSerialDeviceEnumerator.EnumerateDevices().OfType<DeviceDescriptor>().ToList();
        }
    }
}