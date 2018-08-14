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
//using Eyedrivomatic.Device.Commands;

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
                while (_testConnection.ReadMessage(out string msg) && msg.StartsWith("SETTING")){}
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
            VerifyVectorStatus(message, new Vector(0, 0));
        }


        //[Test, Timeout(2500)]
        [TestCase(1000, 100, 100)]
        [TestCase(500, -100, -100)]
        public void Test_Move_BasicallyWorks(int duration, int x, int y)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage($"MOVE {x},{y} {duration}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyMoveStatus(message, new Point(x, y));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration+50)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Move completed in {duration} ms.");
            VerifyVectorStatus(message, new Vector(0, 0));
        }

        [Test]
        public void Test_Move_NewMoveOverrides()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("MOVE 100,100 3000"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyMoveStatus(message, new Point(100, 100));

            Thread.Sleep(1000);
            Assert.That(_testConnection.SendMessage("MOVE -100,-100 1000"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1250)); //Give a few ms for message transmission and processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyMoveStatus(message, new Point(-100, -100));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2250)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, new Vector(0, 0));
        }

        [Test]
        public void Test_Move_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();
            

            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 0,{pos} 100"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                Thread.Sleep(50);
                VerifyMoveStatus(message, new Point(0, pos));
            }
            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE {pos},0 100"), Is.True);
                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                Thread.Sleep(50);

                VerifyMoveStatus(message, new Point(pos, 0));
            }
        }

        [Test, Timeout(2500)]
        [TestCase(-1)]
        [TestCase(10001)]
        public void Test_MoveDurationOutOfRange_RespondsWithError(int duration)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"MOVE 10,10 {duration}"), Is.True);

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

            Assert.That(_testConnection.SendMessage($"MOVE {position},0 0"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: X VALUE OUT OF RANGE {position:f1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyVectorStatus(message, new Vector(0, 0));

            Assert.That(_testConnection.SendMessage($"MOVE 0,{position} 0"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: Y VALUE OUT OF RANGE {position:f1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyVectorStatus(message, new Vector(0, 0));
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

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage($"GO {direction},{speed} {duration} "), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyVectorStatus(message, new Vector(direction, speed));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration + 50)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 0));
        }

        [Test]
        public void Test_Go_NewGoOverrides()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            var start = DateTime.Now;
            Assert.That(_testConnection.SendMessage("GO 0,50 3000 "), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 50));

            Thread.Sleep(1000 - (int)(DateTime.Now - start).TotalMilliseconds);
            Assert.That(_testConnection.SendMessage("GO -180,50 1000"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1100)); //Give a few ms for message processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, new Vector(-180, 50));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2100)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            VerifyVectorStatus(message, new Vector(0,  0));
        }

        [Test]
        public void Test_Go_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            for (var speed = 10; speed < 100; speed += 10)
            {
                for (var direction = 180; direction >= -180; direction--)
                {
                    Assert.That(_testConnection.SendMessage($"GO {direction},{speed} 100"), Is.True);
                    Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                    //Assert.That(message, Is.EqualTo($"STATUS: SERVO_X={pos}({xAbs:F1}),SERVO_Y={-pos}({yAbs:F1}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
                    //Thread.Sleep(10);
                    VerifyVectorStatus(message, new Vector(direction, speed));
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

            Assert.That(_testConnection.SendMessage($"GO 100,10 {duration}"), Is.True);

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

            Assert.That(_testConnection.SendMessage($"GO 0,{speed} 0"), Is.True);

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

            Assert.That(_testConnection.SendMessage($"GO {direction},100 100"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyVectorStatus(message, new Vector(expectedDirection, 100));
        }

        [Test, Timeout(5000)]
        public void Test_SwitchToggle_CanHaveOverlappingTimes()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();


            //switch 1 on
            var start1 = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 1 100"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyMoveStatus(message, new Point(0, 0), true);

            //switch 2 on
            var start2 = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 2 200"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, new Point(0, 0), true, true);

            //switch 3 on
            var start3 = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 3 300"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, new Point(0, 0), true, true, true);

            //switch 4 on
            var start4 = DateTime.Now;
            Assert.That(_testConnection.SendMessage("SWITCH 4 150"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            VerifyMoveStatus(message, new Point(0, 0), true, true, true, true);


            //switch 1 off
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start1).TotalMilliseconds, Is.InRange(100, 125)); //Give a few ms for message transmission and processing
            VerifyMoveStatus(message, new Point(0, 0), false, true, true, true);
            Console.WriteLine($"SWITCH 1 OFF After {(DateTime.Now - start1).TotalMilliseconds}");

            //switch 4 off
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start2).TotalMilliseconds, Is.InRange(150, 175)); //Give a few ms for message transmission and processing
            VerifyMoveStatus(message, new Point(0, 0), false, true, true);
            Console.WriteLine($"SWITCH 4 OFF After {(DateTime.Now - start2).TotalMilliseconds}");

            //switch 2 off
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start3).TotalMilliseconds, Is.InRange(200, 225)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 0), false, false, true);
            Console.WriteLine($"SWITCH 2 OFF After {(DateTime.Now - start3).TotalMilliseconds}");

            //switch 3 off
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start4).TotalMilliseconds, Is.InRange(300, 325)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 0));
            Console.WriteLine($"SWITCH 3 OFF After {(DateTime.Now - start4).TotalMilliseconds}");
        }

        [Test, Timeout(5000)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(5)]
        public void Test_SwitchToggle_OutOfRangeResponseWithError(int switchNumber)
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage($"SWITCH {switchNumber} 0"), Is.True);
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: SWITCH NUMBER OUT OF RANGE {switchNumber}"));
        }

        [Test]
        public void Test_MoveStop_ImmediatelyStops()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("MOVE 100,-100 10000"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyMoveStatus(message, new Point(100, -100));

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 0));
        }

        [Test]
        public void Test_GoStop_ImmediatelyStops()
        {
            _testConnection.ReadStartup();
            _testConnection.EnableLog();

            Assert.That(_testConnection.SendMessage("GO 90,50 10000"), Is.True);

            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            VerifyVectorStatus(message, new Vector(90, 50));

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            VerifyVectorStatus(message, new Vector(0, 0));
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
            VerifyVectorStatus(message, new Vector(0, 0));
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

        private static void VerifyMoveStatus(string message, Point position, bool switch1 = false, bool switch2 = false, bool switch3 = false, bool switch4 = false)
        {
            if (message.StartsWith("STATUS: VECTOR="))
            {
                VerifyVectorStatus(message, position, switch1, switch2, switch3, switch4);
                return;
            }

            var regex = new Regex(@"^STATUS: POS=(?<X>-?\d{1,3}\.\d),(?<Y>-?\d{1,3}\.\d),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF),SWITCH 4=(?<Switch4>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);

            Assert.That(double.Parse(match.Groups["X"].Value), Is.EqualTo(position.X).Within(0.5m));
            Assert.That(double.Parse(match.Groups["Y"].Value), Is.EqualTo(position.Y).Within(0.5m));


            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch4"].Value, Is.EqualTo(switch4 ? "ON" : "OFF"));
        }

        private static void VerifyVectorStatus(string message, Vector vector, bool switch1 = false, bool switch2 = false, bool switch3 = false, bool switch4 = false)
        {
            if (message.StartsWith("STATUS: POS="))
            {
                VerifyMoveStatus(message, vector, switch1, switch2, switch3, switch4);
                return;
            }

            var regex = new Regex(@"^STATUS: VECTOR=(?<DIR>-?\d{1,3}\.\d),(?<SPEED>-?\d{1,3}\.\d),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF),SWITCH 4=(?<Switch4>ON|OFF)$");
            var match = regex.Match(message);
            Assert.That(match.Success, Is.True);

            if (Math.Abs(vector.Direction) == 180.0m)
            {
                Assert.That(Math.Abs(double.Parse(match.Groups["DIR"].Value)), Is.EqualTo(180).Within(1m));
            }
            else
            {
                Assert.That(decimal.Parse(match.Groups["DIR"].Value), Is.EqualTo(vector.Direction).Within(1m));
            }

            Assert.That(decimal.Parse(match.Groups["SPEED"].Value), Is.EqualTo(vector.Speed).Within(1m));

            Assert.That(match.Groups["Switch1"].Value, Is.EqualTo(switch1 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch2"].Value, Is.EqualTo(switch2 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch3"].Value, Is.EqualTo(switch3 ? "ON" : "OFF"));
            Assert.That(match.Groups["Switch4"].Value, Is.EqualTo(switch4 ? "ON" : "OFF"));
        }
    }

    public struct Point
    {
        public decimal X;
        public decimal Y;

        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector(Point point)
        {
            return new Vector(
                (decimal)Math.Atan2((double)point.Y, (double)point.X),
                (decimal)Math.Sqrt(Math.Pow((double)point.X, 2) + Math.Pow((double)point.Y, 2)));
        }

        public override string ToString()
        {
            return $"{X:F1},{X:F1}";
        }

        public static Point Parse(string str)
        {
            var parts = str.Split(',');
            if (parts.Length != 2) throw new ArgumentException();
            if (!decimal.TryParse(parts[0], out decimal x)) throw new ArgumentException();
            if (!decimal.TryParse(parts[1], out decimal y)) throw new ArgumentException();
            return new Point(x, y);
        }
    }

    public struct Vector
    {
        public decimal Direction;
        public decimal Speed;

        public Vector(decimal direction, decimal speed)
        {
            Direction = direction;
            Speed = speed;
        }

        public static implicit operator Point(Vector vector)
        {
            return new Point(
                (decimal)Math.Cos((double)vector.Direction) * vector.Speed,
                (decimal)Math.Sin((double)vector.Direction) * vector.Speed);
        }

        public static Vector Parse(string str)
        {
            var parts = str.Split(',');
            if (parts.Length != 2) throw new ArgumentException();
            if (!decimal.TryParse(parts[0], out decimal direction)) throw new ArgumentException();
            if (!decimal.TryParse(parts[1], out decimal speed)) throw new ArgumentException();
            return new Vector(direction, speed);
        }
    }
}
