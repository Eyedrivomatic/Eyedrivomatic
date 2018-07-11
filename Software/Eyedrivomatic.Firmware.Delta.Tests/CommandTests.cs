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
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;

namespace Eyedrivomatic.Firmware.Delta.Tests
{
    [TestFixture]
    [Explicit("Requires connection to device.")]
    [Category("Firmware")]
    public class CommandTests
    {
        private const int XMin = -22;
        private const int XCenter = 0;
        private const int XMax = 22;
        private const int YMin = -22;
        private const int YCenter = 0;
        private const int YMax = 22;

        private readonly TestConnection _testConnection = new TestConnection();

        [SetUp]
        public void TestInitialize()
        {
            _testConnection.Initialize();
        }

        [TearDown]
        public void TestCleanup()
        {
            _testConnection.Stop();
        }

        [Test, Timeout(5000)]
        public void Test_Startup_StartAndStatusMessagesReceived()
        {
            Assert.That(_testConnection.ReadStartup(), Is.True);
        }

        [Test, Timeout(5000)]
        public void Test_Status_RespondsWithStatus()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("STATUS"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyStatus(message, XCenter, YCenter);
        }


        //[Test, Timeout(2500)]
        [TestCase(1000, 100, 100)]
        [TestCase(500, -100, -100)]
        [TestCase(500, 0, 100)]
        [TestCase(500, 100, 0)]
        public void Test_Move_BasicallyWorks(int duration, int x, int y)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"MOVE {duration} {x} {y}"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyStatus(message, x, y);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration+50)); //Give a few ms for message transmission and processing
            VerifyStatus(message, XCenter, YCenter);
        }

        [Test]
        public void Test_Move_NewMoveOverrides()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("MOVE 3000 100 100"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyStatus(message, 100, 100);

            Thread.Sleep(1000);
            Assert.That(_testConnection.SendMessage("MOVE 1000 -100 -100"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1250)); //Give a few ms for message transmission and processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyStatus(message, -100, -100);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2250)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyStatus(message, XCenter, YCenter);
        }

        [Test]
        public void Test_Move_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog(false);

            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 0 {pos} {-pos}"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));

                //VerifyStatus(message, pos, -pos);
            }
        }

        [Test, Timeout(2500)]
        [TestCase(-1)]
        [TestCase(10001)]
        public void Test_MoveDurationOutOfRange_RespondsWithError(int duration)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"MOVE {duration} 10 10"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: DURATION OUT OF RANGE {duration}"));
        }

        [Test]
        [TestCase(-101)]
        [TestCase(101)]
        public void Test_MovePositionOutOfRange_RespondsWithError(int position)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"MOVE 0 {position} 0"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: XPOS OUT OF RANGE {position}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter);

            Assert.That(_testConnection.SendMessage($"MOVE 0 0 {position}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: YPOS OUT OF RANGE {position}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter);
        }

        [Test, Timeout(5000)]
        public void Test_SwitchToggle_CanHaveOverlappingTimes()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 2000 2"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyStatus(message, XCenter, YCenter, false, true, false);

            Assert.That(_testConnection.SendMessage("SWITCH 1000 1"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter, true, true, false);

            Assert.That(_testConnection.SendMessage("SWITCH 3000 3"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter, true, true, true);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter, false, true, true);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(1000, 1150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter, false, false, true);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(2000, 2150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyStatus(message, XCenter, YCenter);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(3000, 3200)); //Give a few ms for message transmission and processing
        }

        [Test, Timeout(5000)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(4)]
        public void Test_SwitchToggle_OutOfRangeResponseWithError(int switchNumber)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"SWITCH 0 {switchNumber}"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: SWITCH NUMBER OUT OF RANGE {switchNumber}"));
        }

        [Test]
        public void Test_Stop_ImmediatelyStops()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("MOVE 10000 100 -100"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyStatus(message, 100, -100);

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyStatus(message, XCenter, YCenter);
        }

        [Test]
        public void Test_NothingToStop_RespondsWithStatus()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyStatus(message, XCenter, YCenter);
        }

        [Test]
        public void Test_DeviceSendsStartupInfoOnReconnect()
        {
            Assert.That(_testConnection.ReadStartup(), Is.True);
            _testConnection.EnableLog();

            _testConnection.Stop();
            _testConnection.Initialize();

            Assert.That(_testConnection.ReadStartup(), Is.True);
        }

        private static void VerifyStatus(string message, int xRel, int yRel)
        {
            VerifyStatus(message, xRel, yRel, false, false, false);
        }

        private static void VerifyStatus(string message, int x, int y, bool switch1, bool switch2, bool switch3)
        {
            var regex = new Regex(@"^STATUS: POS=(?<X>-?\d{1,3}\.\d),(?<Y>-?\d{1,3}\.\d)\((?<XDevice>-?\d{1,2}\.\d)(?<YDevice>-?\d{1,2}\.\d)\),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);
            Assert.That(int.Parse(match.Groups["X"].Value), Is.EqualTo(x));
            Assert.That(int.Parse(match.Groups["Y"].Value), Is.EqualTo(y));

            var xDevice = x >= 0
                ? XCenter + (XMax - XCenter) * (x / 100m)
                : XCenter + (XCenter - XMin) * (x / 100m);
            xDevice = Math.Round(xDevice, 1, MidpointRounding.AwayFromZero);

            var yDevice = y >= 0
                ? YCenter + (YMax - YCenter) * (y / 100m)
                : YCenter + (YCenter - YMin) * (y / 100m);
            yDevice = Math.Round(yDevice, 1, MidpointRounding.AwayFromZero);

            //Ugly, but there are minor rounding errors that are difficult to account for. Let's just make sure we are within 0.1 deg of expected.
            Assert.That(decimal.Parse(match.Groups["XDevice"].Value), Is.EqualTo(xDevice).Within(0.1m));
            Assert.That(decimal.Parse(match.Groups["YDevice"].Value), Is.EqualTo(yDevice).Within(0.1m));

            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
        }
    }
}
