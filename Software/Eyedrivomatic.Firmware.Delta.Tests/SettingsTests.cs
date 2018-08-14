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


using NUnit.Framework;

namespace Eyedrivomatic.Firmware.Delta.Tests
{
    [TestFixture]
    [Explicit("Requires connection to device.")]
    [Category("Firmware")]
    public class SettingsTests
    {
        private static readonly Point Center = new Point(0, 0);
        private const double MaxSpeed = 16;

        private readonly bool[] _switchStates = { false, false, false, false };

        private readonly TestConnection _testConnection = new TestConnection();

        [SetUp]
        public void TestInitialize()
        {
            _testConnection.Initialize();
        }

        [TearDown]
        public void TestCleanup()
        {
            if (_testConnection.SendMessage("SET DEFAULTS")) _testConnection.ReadMessage(out string _);

            _testConnection.Stop();
        }

        [Test]
        public void Test_GetCenter_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET CENTER"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER {Center.X:F1},{Center.Y:F1}"));
        }

        [Test]
        public void Test_GetMaxSpeed_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET MAX_SPEED"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_SPEED {MaxSpeed:F1}"));
        }

        [Test]
        public void Test_GetOrientation_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET ORIENTATION"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: ORIENTATION 0"));
        }

        [Test]
        public void Test_GetSwitchDefault_Works([Range(1, 4)] int switchNumber)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"GET SWITCH {switchNumber}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH {switchNumber} OFF"));
        }

        [Test]
        public void Test_GetAll_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET ALL"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER {Center.X:F1},{Center.Y:F1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_SPEED {MaxSpeed:F1}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: ORIENTATION 0"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 1 {(_switchStates[0] ? "ON" : "OFF")}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 2 {(_switchStates[1] ? "ON" : "OFF")}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 3 {(_switchStates[2] ? "ON" : "OFF")}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 4 {(_switchStates[3] ? "ON" : "OFF")}"));
        }


        [Test]
        public void Test_SetCenter_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET CENTER -2.5,1"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: CENTER -2.5,1.0"));
        }

        [TestCase(-26)]
        [TestCase(26)]
        public void Test_SetMaxSpeed_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET MAX_SPEED {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value:F1}' is out of range (0.0 to 16.0) for MAX_SPEED"));
        }

        [TestCase(5.5,2)]
        [TestCase(-2,5.5)]
        public void Test_SetCenter_Works(double centerX, double centerY)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER {centerX:F1},{centerY:F1}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER {centerX:F1},{centerY:F1}"));
        }

        [TestCase(-26)]
        [TestCase(26)]
        public void Test_SetCenter_OutOfRange_Is_Ignored(double value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER {value:F1},{value:F1}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value:F1}' is out of range (-16.0 to 16.0) for CENTER Y"));
            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value:F1}' is out of range (-16.0 to 16.0) for CENTER X"));
        }

        [Test]
        public void Test_SetMaxSpeed_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET MAX_SPEED 15.0"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: MAX_SPEED 15.0"));
        }


        [TestCase(0)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        public void Test_SetOrientation_Works(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET ORIENTATION {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: ORIENTATION {value}"));
        }

        public void Test_SetOrientation_Ignores_Invalid_Value()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET ORIENTATION FOO"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("ERROR: 'FOO' is not a valid value for ORIENTATION"));
        }

        [TestCase(91)]
        [TestCase(-1)]
        [TestCase(360)]
        public void Test_SetOrientation_Ignores_OutOfRange(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET ORIENTATION {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is not a valid value for ORIENTATION"));
        }

        [Test]
        public void Test_SetSwitchDefault_Works([Range(1, 4)] int switchNumber)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET SWITCH {switchNumber} ON"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH {switchNumber} ON"));
        }
    }
}
