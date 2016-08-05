using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

using Eyedrivomatic.ButtonDriver.Macros.Models;

namespace Eyedrivomatic.ButtonDriver.Macros.UnitTests
{
    [TestFixture]
    [UseReporter(typeof(VisualStudioReporter))]
    public class MacroSerializationServiceTests
    {
        [Test]
        public void TestSerialize()
        {
            var macros = new[] {
                new UserMacro
                {
                    DisplayName = "TestMacro1",
                    Tasks = 
                    {
                        new CycleRelayTask { DisplayName = "Toggle Relay 1 a few times.", Relay = 1, DelayMs = 100, Repeat = 3 },
                        new DelayTask { DisplayName = "Wait for a second.", DelayMs = 1000 },
                        new CycleRelayTask { DisplayName = "Toggle Relay 2 once.", Relay = 2 }
                    }
                },
                new Macros.Models.UserMacro
                {
                    DisplayName = "TestMacro2"
                },
            };

            var basePath = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var filename = Path.Combine(Path.GetDirectoryName(basePath.LocalPath), "TestMacros.config");
            var serializer = new MacroSerializationService { ConfigurationFilePath = filename };
            serializer.SaveMacros(macros);

            Approvals.VerifyFile(filename);
        }

        [Test]
        public void TestDeserialize()
        {

            var basePath = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var filename = Path.Combine(Path.GetDirectoryName(basePath.LocalPath), @"..\..\MacroSerializationServiceTests.TestSerialize.approved.config");
            var serializer = new MacroSerializationService { ConfigurationFilePath = filename };

            var macros = serializer.LoadMacros().ToArray();

            var expected = new[] {
                    new UserMacro
                    {
                        DisplayName = "TestMacro1",
                        Tasks =
                        {
                            new CycleRelayTask { DisplayName = "Toggle Relay 1 a few times.", Relay = 1, DelayMs = 100, Repeat = 3 },
                            new DelayTask { DisplayName = "Wait for a second.", DelayMs = 1000 },
                            new CycleRelayTask { DisplayName = "Toggle Relay 2 once.", Relay = 2 }
                        }
                    } as IMacro,
                    new Macros.Models.UserMacro
                    {
                        DisplayName = "TestMacro2"
                    } as IMacro,
                };

            Assert.That(macros, Is.EquivalentTo(expected));
        }
    }
}
