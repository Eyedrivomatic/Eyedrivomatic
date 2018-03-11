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
using System.Reactive;
using System.Reactive.Linq;
using Eyedrivomatic.Hardware.Services;
using NUnit.Framework;

namespace Eyedrivomatic.Hardware.UnitTests
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

            sut.Attach("STATUS: SERVO_X=13(4.0),SERVO_Y=-13(-4.0),SWITCH 1=OFF,SWITCH 2=ON,SWITCH 3=OFF#31\n".ToObservable(),
                new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));

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

            sut.Attach("STATUS: FOO\n".ToObservable(),
                new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));

            Assert.That(parseError, Is.True);
        }
    }
}
