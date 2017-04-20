using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [MessageProcessor("Log")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class LogMessageProcessor : LineMessageProcessor
    {
        public const string MessagePrefix = "LOG:";
        private readonly Regex _messageFormat = new Regex($@"^{MessagePrefix} (?<Severity>.*) - (?<Message>.*)$");

        private void Process(string message)
        {
            var match = _messageFormat.Match(message);
            if (!match.Success)
            {
                ButtonDriverHardwareModule.Logger?.Log($"Unable to parse log entry - '{message}'.", Category.Warn, Priority.None);
                return;
            }

            var categoryMap = new Dictionary<string, Category>
            {
                { "DEBUG", Category.Debug },
                { "INFO", Category.Info },
                { "WARN", Category.Warn },
                { "ERROR", Category.Exception },
            };

            var category =
                (match.Groups["Severity"].Success && categoryMap.ContainsKey(match.Groups["Severity"].Value))
                    ? categoryMap[match.Groups["Severity"].Value]
                    : Category.Exception;

            ButtonDriverHardwareModule.Logger?.Log(match.Groups["Message"].Value, category, Priority.None);
        }

        protected override IDisposable Attach(IObservable<string> source)
        {
            return source.Where(message => message.StartsWith(MessagePrefix)).Subscribe(Process);
        }
    }
}
