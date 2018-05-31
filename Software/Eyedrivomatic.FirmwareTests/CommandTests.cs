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
using NUnit.Framework;
using System.Threading;

namespace FirmwareTests
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
            Assert.That(_testConnection.SendMessage($"MOVE {duration} {x} {y}"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyStatus(message, x, y);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration+50)); //Give a few ms for message transmission and processing
            VerifyStatus(message, XCenter, YCenter);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_Move_Invert_IsApplied(bool invertBothAxis)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET INVERT_X {(invertBothAxis ? "ON" : "OFF")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_X {(invertBothAxis ? "ON" : "OFF")}"));

            Assert.That(_testConnection.SendMessage($"SET INVERT_Y {(invertBothAxis ? "ON" : "OFF")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_Y {(invertBothAxis ? "ON" : "OFF")}"));


            Assert.That(_testConnection.SendMessage("MOVE 1000 50 -50"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);

            //The status does not show a difference.
            VerifyStatus(message, 50, -50);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);

            Assert.That(_testConnection.SendMessage("MOVE 1000 -50 50"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);

            //The status does not show a difference.
            VerifyStatus(message, -50, 50);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
        }

        [Test]
        public void Test_Move_NewMoveOverrides()
        {
            _testConnection.ReadStartup();
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

            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 0 {pos} {-pos}"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));

                VerifyStatus(message, pos, -pos);
            }
        }

        [Test, Timeout(2500)]
        [TestCase(-1)]
        [TestCase(10001)]
        public void Test_MoveDurationOutOfRange_RespondsWithError(int duration)
        {
            _testConnection.ReadStartup();
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

            Assert.That(_testConnection.SendMessage("SWITCH 2000 2"), Is.True);
            var start = DateTime.Now;
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


            Assert.That(_testConnection.SendMessage($"SWITCH 0 {switchNumber}"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: SWITCH NUMBER OUT OF RANGE {switchNumber}"));
        }

        [Test]
        public void Test_Stop_ImmediatelyStops()
        {
            _testConnection.ReadStartup();
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
        
            var start = DateTime.Now;

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyStatus(message, XCenter, YCenter);
        }

        private static void VerifyStatus(string message, int xRel, int yRel)
        {
            VerifyStatus(message, xRel, yRel, false, false, false);
        }

        private static void VerifyStatus(string message, int xRel, int yRel, bool switch1, bool switch2, bool switch3)
        {
            var regex = new Regex(@"^STATUS: SERVO_X=(?<XRelative>-?\d+)\((?<XAbsolute>-?\d{1,3}\.\d)\),SERVO_Y=(?<YRelative>-?\d+)\((?<YAbsolute>-?\d{1,3}\.\d)\),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);
            Assert.That(int.Parse(match.Groups["XRelative"].Value), Is.EqualTo(xRel));
            Assert.That(int.Parse(match.Groups["YRelative"].Value), Is.EqualTo(yRel));

            var xAbs = xRel >= 0
                ? XCenter + (XMax - XCenter) * (xRel / 100m)
                : XCenter + (XCenter - XMin) * (xRel / 100m);
            xAbs = Math.Round(xAbs, 1, MidpointRounding.AwayFromZero);

            var yAbs = yRel >= 0
                ? YCenter + (YMax - YCenter) * (yRel / 100m)
                : YCenter + (YCenter - YMin) * (yRel / 100m);
            yAbs = Math.Round(yAbs, 1, MidpointRounding.AwayFromZero);

            //Ugly, but there are minor rounding errors that are difficult to account for. Let's just make sure we are within 0.2 deg of expected.
            Assert.That(decimal.Parse(match.Groups["XAbsolute"].Value), Is.EqualTo(xAbs).Within(0.1m));
            Assert.That(decimal.Parse(match.Groups["YAbsolute"].Value), Is.EqualTo(yAbs).Within(0.1m));

            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
        }
    }
}
