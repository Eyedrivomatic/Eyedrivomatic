using System;
using System.Collections.ObjectModel;
using System.Reflection;

using ApprovalTests.Reporters;
using NUnit.Framework;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using ApprovalTests;
using System.IO;

namespace Eyedrivomatic.ButtonDriver.Macros.UnitTests
{
    [TestFixture]
    public class MacroSerializationServiceTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TestSerialize()
        {
            var macros = new[] {
                new UserMacro
                {
                    DisplayName = "TestMacro1",
                    Tasks = new ObservableCollection<Models.MacroTask>
                    {
                        new ToggleRelayTask { DisplayName = "Toggle Relay 1 a few times.", Relay = 1, DelayMs = 100, Repeat = 3 },
                        new DelayTask { DisplayName = "Wait for a second.", DelayTimeMs = 1000 },
                        new ToggleRelayTask { DisplayName = "Toggle Relay 2 once.", Relay = 2 }
                    }
                },
                new Macros.Models.UserMacro
                {
                    DisplayName = "TestMacro2"
                },
            };

            var basePath = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var filename = Path.Combine(Path.GetDirectoryName(basePath.LocalPath), "TestMacros.config");
            var serializer = new MacroSerializationService { MacrosPath = filename };

            Approvals.VerifyFile(filename);
        }
    }
}
