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
using System.Linq;
using Eyedrivomatic.Device.Services;
using NUnit.Framework;

namespace Eyedrivomatic.Device.Serial.UnitTests
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
