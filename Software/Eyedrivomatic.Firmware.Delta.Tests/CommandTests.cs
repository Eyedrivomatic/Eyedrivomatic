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
        private readonly TestConnection _testConnection = new TestConnection();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testConnection.Initialize();
            if (_testConnection.SendMessage("SET DEFAULTS"))
            {
                while (_testConnection.ReadMessage(out string msg) && msg.StartsWith("SETTING")){};
            }
            _testConnection.Stop();
            Thread.Sleep(100);
        }

        [SetUp]
        public void TestInitialize()
        {
            _testConnection.Initialize();
        }

        [TearDown]
        public void TestCleanup()
        {
            _testConnection.Stop();
            Thread.Sleep(100);
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
            VerifyVectorStatus(message, 0, 0);
        }


        //[Test, Timeout(2500)]
        [TestCase(1000, 100, 100)]
        [TestCase(500, -100, -100)]
        public void Test_Move_BasicallyWorks(int duration, int x, int y)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage($"MOVE {duration} {x} {y}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyMoveStatus(message, x, y);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration+50)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Move completed in {duration} ms.");
            VerifyVectorStatus(message, 0, 0);
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
            VerifyMoveStatus(message, 100, 100);

            Thread.Sleep(1000);
            Assert.That(_testConnection.SendMessage("MOVE 1000 -100 -100"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1250)); //Give a few ms for message transmission and processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyMoveStatus(message, -100, -100);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2250)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, 0, 0);
        }

        [Test]
        public void Test_Move_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog(true);
            

            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 100 0 {pos}"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                Thread.Sleep(50);
                VerifyMoveStatus(message, 0, pos);
            }
            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 100 {pos} 0"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                Thread.Sleep(50);

                VerifyMoveStatus(message, pos, 0);
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
            Assert.That(message, Is.EqualTo($"ERROR: XPOS OUT OF RANGE {position:f1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyVectorStatus(message, 0, 0);

            Assert.That(_testConnection.SendMessage($"MOVE 0 0 {position}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: YPOS OUT OF RANGE {position:f1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyVectorStatus(message, 0, 0);
        }


        [TestCase(500, 0, 100)]
        [TestCase(500, 1, 100)]
        [TestCase(500, 90, 100)]
        [TestCase(500, 179, 100)]
        [TestCase(500, 180, 100)]
        [TestCase(500, -180, 100)]
        [TestCase(500, -179, 100)]
        [TestCase(500, -90, 100)]
        [TestCase(1000, -1, 100)]
        public void Test_Go_BasicallyWorks(int duration, int direction, int speed)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"GO {direction} {speed} {duration} "), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyVectorStatus(message, direction, speed);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration + 50)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, 0, 0);
        }

        [Test]
        public void Test_Go_NewGoOverrides()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage("GO 0 50 3000 "), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, 0, 50);

            Thread.Sleep(1000 - (int)(DateTime.Now - start).TotalMilliseconds);
            Assert.That(_testConnection.SendMessage("GO -180 50 1000"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1100)); //Give a few ms for message processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, -180, 50);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2100)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, 0,  0);
        }

        [Test]
        public void Test_Go_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog(true);

            for (var speed = 10; speed < 100; speed += 10)
            {
                for (var direction = 180; direction >= -180; direction--)
                {
                    Assert.That(_testConnection.SendMessage($"GO {direction} {speed} 100"), Is.True);
                    Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                    //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                    //Thread.Sleep(10);
                    VerifyVectorStatus(message, direction, speed);
                }
            }
        }

        [Test, Timeout(2500)]
        [TestCase(-1)]
        [TestCase(10001)]
        public void Test_GoDurationOutOfRange_RespondsWithError(int duration)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"GO 100 10 {duration}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: DURATION OUT OF RANGE {duration}"));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(101)]
        public void Test_GoSpeedOutOfRange_RespondsWithError(int speed)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"GO 0 {speed} 0"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: SPEED OUT OF RANGE {speed:f1}"));
        }


        [Test]
        [TestCase(-181, 179)]
        [TestCase(181, -179)]
        public void Test_GoDirectionOutOfRange_GoesAround(int direction, int expectedDirection)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"GO {direction} 100 100"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyVectorStatus(message, expectedDirection, 100);
        }




        [Test, Timeout(5000)]
        public void Test_SwitchToggle_CanHaveOverlappingTimes()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 2000 2"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyMoveStatus(message, 0, 0, false, true, false);

            Assert.That(_testConnection.SendMessage("SWITCH 1000 1"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, 0, 0, true, true, false);

            Assert.That(_testConnection.SendMessage("SWITCH 3000 3"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, 0, 0, true, true, true);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, 0, 0, false, true, true);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(1000, 1150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, 0, 0, false, false, true);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(2000, 2150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyVectorStatus(message, 0, 0);
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
            VerifyMoveStatus(message, 100, -100);

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, 0, 0);
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
            VerifyVectorStatus(message, 0, 0);
        }

        [Test]
        public void Test_DeviceSendsStartupInfoOnReconnect()
        {
            Assert.That(_testConnection.ReadStartup(), Is.True);
            _testConnection.EnableLog();

            _testConnection.Stop();
            Thread.Sleep(100);
            _testConnection.Initialize();

            Assert.That(_testConnection.ReadStartup(), Is.True);
        }

        private static void VerifyMoveStatus(string message, decimal x, decimal y, bool switch1 = false, bool switch2 = false, bool switch3 = false, bool switch4 = false)
        {
            var regex = new Regex(@"^STATUS: POS=(?<X>-?\d{1,3}\.\d),(?<Y>-?\d{1,3}\.\d),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF),SWITCH 4=(?<Switch4>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);

            Assert.That(decimal.Parse(match.Groups["X"].Value), Is.EqualTo(x).Within(0.2m));
            Assert.That(decimal.Parse(match.Groups["Y"].Value), Is.EqualTo(y).Within(0.2m));


            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch4"].Value, Is.EqualTo(switch4 ? "ON" : "OFF"));
        }

        private static void VerifyVectorStatus(string message, decimal direction, decimal speed, bool switch1 = false, bool switch2 = false, bool switch3 = false, bool switch4 = false)
        {
            var regex = new Regex(@"^STATUS: VECTOR=(?<DIR>-?\d{1,3}\.\d),(?<SPEED>-?\d{1,3}\.\d),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF),SWITCH 4=(?<Switch4>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);

            if (Math.Abs(direction) == 180)
            {
                Assert.That(Math.Abs(decimal.Parse(match.Groups["DIR"].Value)), Is.EqualTo(180).Within(1m));
            }
            else
            {
                Assert.That(decimal.Parse(match.Groups["DIR"].Value), Is.EqualTo(direction).Within(1m));
            }

            Assert.That(decimal.Parse(match.Groups["SPEED"].Value), Is.EqualTo(speed).Within(1m));

            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch4"].Value, Is.EqualTo(switch4 ? "ON" : "OFF"));
        }
    }
}
