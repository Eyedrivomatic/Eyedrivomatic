using NUnit.Framework;

namespace FirmwareTests
{
    [TestFixture]
    public class SettingsTests
    {
        private const int XMin = -22;
        private const int XCenter = 0;
        private const int XMax = 22;
        //private const bool XInvert = false;
        private const int YMin = -22;
        private const int YCenter = 0;
        private const int YMax = 22;
        //private const bool YInvert = true;
        private readonly bool[] _switchStates = { false, false, false };

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
        public void Test_GetMinX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET MIN_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_X {XMin}"));
        }

        [Test]
        public void Test_GetCenterX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET CENTER_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X {XCenter}"));
        }

        [Test]
        public void Test_GetMaxX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET MAX_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_X {XMax}"));
        }

        [Test]
        public void Test_GetInvertX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET INVERT_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: INVERT_X ON"));
        }

        [Test]
        public void Test_GetMinY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET MIN_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_Y {YMin}"));
        }

        [Test]
        public void Test_GetCenterY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET CENTER_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y {YCenter}"));
        }

        [Test]
        public void Test_GetMaxY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET MAX_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_Y {YMax}"));
        }

        [Test]
        public void Test_GetInvertY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("GET INVERT_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: INVERT_Y OFF"));
        }


        [Test]
        public void Test_GetSwitchDefault_Works([Range(1, 3)] int switchNumber)
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
            Assert.That(message, Is.EqualTo($"SETTING: MIN_X {XMin}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X {XCenter}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_X {XMax}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: INVERT_X ON"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_Y {YMin}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y {YCenter}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_Y {YMax}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: INVERT_Y OFF"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 1 {(_switchStates[0] ? "ON" : "OFF")}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 2 {(_switchStates[1] ? "ON" : "OFF")}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH 3 {(_switchStates[2] ? "ON" : "OFF")}"));
        }


        [Test]
        public void Test_SetMinX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET MIN_X -15"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: MIN_X -15"));
        }

        [TestCase(-26)]
        [TestCase(1)]
        public void Test_SetMinX_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET MIN_X {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (-22 to 0) for MIN_X"));
        }

        [TestCase(5)]
        [TestCase(-5)]
        public void Test_SetCenterX_Works(int center)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER_X {center}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X {center}"));
        }

        [TestCase(-26)]
        [TestCase(26)]
        public void Test_SetCenterX_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER_X {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (-22 to 22) for CENTER_X"));
        }

        [Test]
        public void Test_SetMaxX_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET MAX_X 15"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: MAX_X 15"));
        }

        [TestCase(-1)]
        [TestCase(26)]
        public void Test_SetMaxX_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET MAX_X {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (0 to 22) for MAX_X"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_SetInvertX_Works(bool value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET INVERT_X {(value ? "ON" : "OFF")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_X {(value ? "ON" : "OFF")}"));
        }

        [Test]
        public void Test_SetInvertX_Ignores_Invalid_Value()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET INVERT_X FOO"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("ERROR: 'FOO' is not a valid value for INVERT_X"));
        }

        [Test]
        public void Test_SetMinY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET MIN_Y -15"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: MIN_Y -15"));
        }

        [TestCase(-26)]
        [TestCase(1)]
        public void Test_SetMinY_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET MIN_Y {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (-22 to 0) for MIN_Y"));
        }

        [TestCase(5)]
        [TestCase(-5)]
        public void Test_SetCenterY_Works(int center)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER_Y {center}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y {center}"));
        }

        [TestCase(-26)]
        [TestCase(26)]
        public void Test_SetCenterY_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET CENTER_Y {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (-22 to 22) for CENTER_Y"));
        }

        [Test]
        public void Test_SetMaxY_Works()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET MAX_Y 15"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("SETTING: MAX_Y 15"));
        }

        [TestCase(-1)]
        [TestCase(26)]
        public void Test_SetMaxY_OutOfRange_Is_Ignored(int value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET MAX_Y {value}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"ERROR: '{value}' is out of range (0 to 22) for MAX_Y"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_SetInvertY_Works(bool value)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET INVERT_Y {(value ? "ON" : "OFF")}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: INVERT_Y {(value ? "ON" : "OFF")}"));
        }

        [Test]
        public void Test_SetInvertY_Ignores_Invalid_Value()
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage("SET INVERT_Y FOO"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo("ERROR: 'FOO' is not a valid value for INVERT_Y"));
        }

        [Test]
        public void Test_SetSwitchDefault_Works([Range(1, 3)] int switchNumber)
        {
            _testConnection.ReadStartup();

            Assert.That(_testConnection.SendMessage($"SET SWITCH {switchNumber} ON"), Is.True);

            Assert.That(_testConnection.ReadMessage(out string message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH {switchNumber} ON"));
        }
    }
}
