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
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    public class BrainBoxPortFinder
    {
        private static readonly string[] DeviceUsbVids = new[] { "2341", "2A03" }; //The Arduino (2341) and Genuino (2A03) Vid's
        private static readonly string[] DeviceUsbPids = new[] { "0001", "0043", "0243" }; //The known Arduino Uno Pid's

        private static readonly string StartupMessage = "START: Eyedrivomatic - version 2.0.0";

        private static ILoggerFacade Logger => ServiceLocator.Current.GetInstance<ILoggerFacade>();

        public static async Task<SerialPort> DetectDeviceAsync(CancellationToken cancellationToken)
        {
            var devices = UsbSerialDeviceEnumerator.EnumerateDevices().ToList();
            cancellationToken.ThrowIfCancellationRequested();

            if (!devices.Any()) return null;

            var tasks = (from device in devices
                         where DeviceUsbVids.Contains(device.Vid) && DeviceUsbPids.Contains(device.Pid)
                         select OpenSerialPortAsync(device.Port)).ToList();

            var cancelTcs = new TaskCompletionSource<SerialPort>();

            using (cancellationToken.Register(() => cancelTcs.SetCanceled()))
            while (tasks.Any())
            {
                var resultTask = await Task.WhenAny(tasks.Union(new[] {cancelTcs.Task}));
                cancellationToken.ThrowIfCancellationRequested();

                if (resultTask.IsCompleted && resultTask.Result != null)
                {
                    Logger.Log($"Found device on port [{resultTask.Result.PortName}]!", Category.Info, Priority.None);
                    return resultTask.Result;
                }

                tasks.Remove(resultTask);
            }

            return null;
        }

        public static IEnumerable<Tuple<string, string>> GetAvailableDevices()
        {
            return from device in UsbSerialDeviceEnumerator.EnumerateDevices()
            select new Tuple<string, string>(device.FriendlyName, device.Port);
        }

        public static async Task<SerialPort> OpenSerialPortAsync(string port)
        {
            try
            {
                Logger.Log($"Opening port [{port}].", Category.Info, Priority.None);
                var serialPort = new SerialPort(port, 19200)
                {
                    DtrEnable = false,
                    ReadTimeout = 5000
                };

                serialPort.Open();

                serialPort.DiscardInBuffer();
                serialPort.DtrEnable = true;//this will reset the Arduino.

                if (!serialPort.IsOpen) return null;
                if (!await VerifyStartupMessage(serialPort))
                {
                    Logger.Log($"Device not found on port [{port}]", Category.Info, Priority.None);
                    serialPort.Dispose();
                    return null;
                };

                return serialPort;
            }
            catch (UnauthorizedAccessException)
            {
                // Access is denied to the port. 
                // -or -
                // The current process, or another process on the system, already has the specified COM port open either by a SerialPort instance or in unmanaged code.
                Logger.Log($"COM port [{port}] is in use.", Category.Exception, Priority.None);
                return null;
            }
            catch (ArgumentException)
            {
                //Configuration shold start with "COM"
                Logger.Log($"Invalid port name [{port}].", Category.Exception, Priority.None);
                return null;
            }
            catch (IOException ex)
            {
                //The port is in an invalid state.
                // -or -
                //An attempt to set the state of the underlying port failed. For example, the parameters passed from this SerialPort object were invalid.
                Logger.Log($"Failed to open the com port [{ex}]", Category.Exception, Priority.None);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log($"Failed to open the com port [{ex}]", Category.Exception, Priority.None);
                return null;
            }
        }

        private static async Task<bool> VerifyStartupMessage(SerialPort serialPort)
        {
            var reader = new StreamReader(serialPort.BaseStream, Encoding.ASCII); //Do not dispose. It will close the underlying stream.
            var firstMessage = await reader.ReadLineAsync();
            Logger.Log($"First message on port [{serialPort.PortName}] is [{firstMessage}].", Category.Debug, Priority.None);

            //Ignore the checksum... TODO: Don't ignore the checksum.
            return string.CompareOrdinal(firstMessage.Substring(0, StartupMessage.Length), StartupMessage) == 0;
        }
    }
}
