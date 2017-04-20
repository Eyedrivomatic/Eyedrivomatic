using NUnit.Framework;

namespace FirmwareTests
{
    [TestFixture]
    public class SettingsTests
    {
        private const uint XMin = 60;
        private const uint XCenter = 90;
        private const uint XMax = 120;
        private const uint YMin = 60;
        private const uint YCenter = 90;
        private const uint YMax = 120;
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
            string message;
            if (_testConnection.SendMessage($"SET DEFAULTS")) _testConnection.ReadMessage(out message);

            _testConnection.Stop();
        }


        [Test]
        public void Test_GetMinX_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET MIN_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_X {XMin}"));
        }

        [Test]
        public void Test_GetCenterX_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET CENTER_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X {XCenter}"));
        }

        [Test]
        public void Test_GetMaxX_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET MAX_X"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_X {XMax}"));
        }

        [Test]
        public void Test_GetMinY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage($"GET MIN_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_Y {YMin}"));
        }

        [Test]
        public void Test_GetCenterY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET CENTER_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y {YCenter}"));
        }

        [Test]
        public void Test_GetMaxY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET MAX_Y"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_Y {YMax}"));
        }

        [Test]
        public void Test_GetSwitchDefault_Works([Range(1, 3)] int switchNumber)
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage($"GET SWITCH {switchNumber}"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH {switchNumber} OFF"));
        }

        [Test]
        public void Test_GetAll_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("GET ALL"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_X {XMin}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X {XCenter}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_X {XMax}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_Y {YMin}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y {YCenter}"));

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_Y {YMax}"));

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

            string message;
            Assert.That(_testConnection.SendMessage("SET MIN_X 70"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_X 70"));
        }

        [Test]
        public void Test_SetCenterX_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("SET CENTER_X 80"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_X 80"));
        }

        [Test]
        public void Test_SetMaxX_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("SET MAX_X 110"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_X 110"));
        }

        [Test]
        public void Test_SetMinY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage($"SET MIN_Y 70"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MIN_Y 70"));
        }

        [Test]
        public void Test_SetCenterY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("SET CENTER_Y 80"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: CENTER_Y 80"));
        }

        [Test]
        public void Test_SetMaxY_Works()
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage("SET MAX_Y 110"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: MAX_Y 110"));
        }

        [Test]
        public void Test_SetSwitchDefault_Works([Range(1, 3)] int switchNumber)
        {
            _testConnection.ReadStartup();

            string message;
            Assert.That(_testConnection.SendMessage($"SET SWITCH {switchNumber} ON"), Is.True);

            Assert.That(_testConnection.ReadMessage(out message), Is.True);
            Assert.That(message, Is.EqualTo($"SETTING: SWITCH {switchNumber} ON"));
        }
    }
}
