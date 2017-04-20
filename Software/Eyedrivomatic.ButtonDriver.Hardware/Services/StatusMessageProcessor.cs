using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IStatusMessageSource))]
    [MessageProcessor("Status")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class StatusMessageProcessor : LineMessageProcessor, IStatusMessageSource
    {
        public const string MessagePrefix = "STATUS:";
        private readonly Regex _messageFormat = new Regex($@"^{MessagePrefix} SERVO_X=(?<XRelative>-?\d{{1,3}})\((?<XAbsolute>\d{{3}})\),SERVO_Y=(?<YRelative>-?\d{{1,3}})\((?<YAbsolute>\d{{3}})\),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF)$");

        internal void Process(string message)
        {
            if (!ChecksumProcessor.ValidateChecksum(ref message))
            {
                ButtonDriverHardwareModule.Logger?.Log($"Invalid checksum for message - '{message}'.", Category.Warn, Priority.None);
                StatusParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            var match = _messageFormat.Match(message);
            if (!match.Success)
            {
                ButtonDriverHardwareModule.Logger?.Log($"Unable to parse status message - '{message}'.", Category.Warn, Priority.None);
                StatusParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            StatusMessageReceived?.Invoke(this, 
                new StatusMessageEventArgs(
                    Convert.ToInt32(match.Groups["XRelative"].Value),
                    Convert.ToInt32(match.Groups["XAbsolute"].Value),
                    Convert.ToInt32(match.Groups["YRelative"].Value),
                    Convert.ToInt32(match.Groups["YAbsolute"].Value),
                    match.Groups["Switch1"].Value == "ON",
                    match.Groups["Switch2"].Value == "ON",
                    match.Groups["Switch3"].Value == "ON"));
        }

        protected override IDisposable Attach(IObservable<string> source)
        {
            return source.Where(message => message.StartsWith(MessagePrefix)).Subscribe(Process, OnCompleted);
        }

        private void OnCompleted()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler<StatusMessageEventArgs> StatusMessageReceived;
        public event EventHandler StatusParseError;
        public event EventHandler Disconnected;
    }
}
