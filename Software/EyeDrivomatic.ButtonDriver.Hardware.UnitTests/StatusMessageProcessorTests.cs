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
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Eyedrivomatic.ButtonDriver.Hardware.Services;


namespace EyeDrivomatic.ButtonDriver.Hardware.UnitTests
{
    [TestFixture]
    public class StatusMessageProcessorTests
    {
        [Test]
        public void TestStatusMessageProcessor_Works()
        {
            var sut = new StatusMessageProcessor();

            StatusMessageEventArgs receievedArgs = null;
            sut.StatusMessageReceived += (sender, args) => receievedArgs = args;
            sut.StatusParseError += (sender, args) => Assert.Fail($"Parse Failed");

            sut.Attach("STATUS: SERVO_X=13(4.0),SERVO_Y=-13(-4.0),SWITCH 1=OFF,SWITCH 2=ON,SWITCH 3=OFF#31\n".ToObservable());

            Assert.That(receievedArgs, Is.Not.Null);
            Assert.That(receievedArgs.XRelative, Is.EqualTo(13));
            Assert.That(receievedArgs.XAbsolute, Is.EqualTo(4d).Within(0.1));
            Assert.That(receievedArgs.YRelative, Is.EqualTo(-13));
            Assert.That(receievedArgs.YAbsolute, Is.EqualTo(-4d).Within(0.1));
            Assert.That(receievedArgs.Switch1, Is.False);
            Assert.That(receievedArgs.Switch2, Is.True);
            Assert.That(receievedArgs.Switch3, Is.False);
        }

        [Test]
        public void TestStatusMessageProcessor_ReturnsFalseIfParseFails()
        {
            var sut = new StatusMessageProcessor();
            var parseError = false;
            sut.StatusParseError += (sender, args) => parseError = true;

            sut.Attach("STATUS: FOO\n".ToObservable());

            Assert.That(parseError, Is.True);
        }
    }
}
