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
using System.Linq;
using NUnit.Framework;

namespace Eyedrivomatic.Hardware.UnitTests
{
    [TestFixture]
    public class UsbSerialDeviceTests
    {
        [Test]
        [TestCase(new [] {"2341", "2A03" }, new[] { "0043", "0001", "0243" })] //Arduino Uno
        //[Ignore("Requires USB serial device to be attached.")]
        public void GetPortFromPidVidTest(string[] vids, string[] pids)
        {
            var devices = UsbSerialDeviceEnumerator.EnumerateDevices().ToList();

            devices.ForEach(device => Console.WriteLine($"Found {device.FriendlyName} on port {device.ConnectionString} - VID:{device.Vid}, PID:{device.Pid}"));
            Assert.That(devices.Any(device => vids.Contains(device.Vid) && pids.Contains(device.Pid)), Is.True);
        }
    }
}
