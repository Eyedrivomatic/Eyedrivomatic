using System;
using NUnit.Framework;
using System.Threading;

namespace FirmwareTests
{
    [TestFixture]
    public class CommandTests
    {
        private const uint XMin = 60;
        private const uint XCenter = 90;
        private const uint XMax = 120;
        private const uint YMin = 60;
        private const uint YCenter = 90;
        private const uint YMax = 120;

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
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }
        

        [Test, Timeout(2500)]
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
            Assert.That(message, Does.Match(@"STATUS: SERVO_X=-?\d+\(\d{3}\),SERVO_Y=-?\d+\(\d{3}\),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF")); //Testing servo position calculations in TestMoveRange.

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(duration, duration+50)); //Give a few ms for message transmission and processing
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_Move_Invert_IsApplied(bool invertBothAxis)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET INVERT_X {(invertBothAxis ? "TRUE" : "FALSE")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_X {(invertBothAxis ? "TRUE" : "FALSE")}"));

            Assert.That(_testConnection.SendMessage($"SET INVERT_Y {(invertBothAxis ? "TRUE" : "FALSE")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_Y {(invertBothAxis ? "TRUE" : "FALSE")}"));


            Assert.That(_testConnection.SendMessage("MOVE 100 50 -50"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);

            Assert.That(message,
                invertBothAxis
                    ? Is.EqualTo(@"STATUS: SERVO_X=50(075),SERVO_Y=-50(105),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF")
                    : Is.EqualTo(@"STATUS: SERVO_X=50(105),SERVO_Y=-50(075),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));

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
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=100({XMax:D3}),SERVO_Y=100({YMax:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));

            Thread.Sleep(1000);
            Assert.That(_testConnection.SendMessage("MOVE 1000 -100 -100"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            var responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(1000, 1150)); //Give a few ms for message transmission and processing, but not enough for the first move command to complete.
            Console.WriteLine($"Message received in {responseTime} ms.");
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=-100({XMin:D3}),SERVO_Y=-100({YMin:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));


            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            responseTime = (DateTime.Now - start).TotalMilliseconds;
            Assert.That(responseTime, Is.InRange(2000, 2150)); //Give a few ms for message transmission and processing
            Console.WriteLine($"Message received in {responseTime} ms.");
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }

        [Test]
        public void Test_Move_RangeAndExpectedRounding()
        {
            _testConnection.ReadStartup();

            for (var pos = -100; pos <= 100; pos++)
            {
                Assert.That(_testConnection.SendMessage($"MOVE 0 {pos} {-pos}"), Is.True);

                var xAbs = Math.Round(pos > 0
                    ? XCenter + (XMax - XCenter)*(pos/100d)
                    : XCenter + (XCenter - XMin)*(pos/100d), MidpointRounding.AwayFromZero);

                var yAbs = Math.Round(-pos > 0
                    ? YCenter + (YMax - YCenter)*(-pos/100d)
                    : YCenter + (YCenter - YMin)*(-pos/100d), MidpointRounding.AwayFromZero);

                var xRel = Math.Round(xAbs > XCenter
                    ? 100d*(xAbs - XCenter)/(XMax - XCenter)
                    : 100d*(xAbs - XCenter)/(XCenter - XMin), MidpointRounding.AwayFromZero);

                var yRel = Math.Round(yAbs > YCenter
                    ? 100d*(yAbs - YCenter)/(YMax - YCenter)
                    : 100d*(yAbs - YCenter)/(YCenter - YMin), MidpointRounding.AwayFromZero);

                Assert.That(_testConnection.ReadMessage(out string message), Is.True);
                Assert.That(message,
                    Is.EqualTo(
                        $"STATUS: SERVO_X={xRel}({xAbs:000}),SERVO_Y={yRel}({yAbs:000}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
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
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));

            Assert.That(_testConnection.SendMessage($"MOVE 0 0 {position}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: YPOS OUT OF RANGE {position}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }

        [Test, Timeout(5000)]
        public void Test_SwitchToggle_CanHaveOverlappingTimes()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SWITCH 2000 2"), Is.True);
            var start = DateTime.Now;
            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=ON,SWITCH 3=OFF"));

            Assert.That(_testConnection.SendMessage("SWITCH 1000 1"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=ON,SWITCH 2=ON,SWITCH 3=OFF"));

            Assert.That(_testConnection.SendMessage("SWITCH 3000 3"), Is.True);
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=ON,SWITCH 2=ON,SWITCH 3=ON"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=ON,SWITCH 3=ON"));
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(1000, 1150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=ON"));
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(2000, 2150)); //Give a few ms for message transmission and processing

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
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
            Assert.That(message, Does.StartWith("STATUS:"));

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }

        [Test]
        public void Test_NothingToStop_RespondsWithStatus()
        {
            _testConnection.ReadStartup();
        
            var start = DateTime.Now;

            Assert.That(_testConnection.SendMessage("STOP"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That((DateTime.Now - start).TotalMilliseconds, Is.InRange(0, 100)); //Give a few ms for message transmission and processing
            Assert.That(message, Is.EqualTo($"STATUS: SERVO_X=0({XCenter:D3}),SERVO_Y=0({XCenter:D3}),SWITCH 1=OFF,SWITCH 2=OFF,SWITCH 3=OFF"));
        }
    }
}
