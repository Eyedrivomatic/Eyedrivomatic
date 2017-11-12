using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(ISettingsMessageSource))]
    [MessageProcessor("Settings")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class SettingMessageProcessor : LineMessageProcessor, ISettingsMessageSource
    {
        public const string MessagePrefix = "SETTING:";
        private readonly Regex _messageFormat = new Regex($@"^{MessagePrefix} (?<Name>.+) (?<Value>.+)$");

        private void Process(string message)
        {
            if (!ChecksumProcessor.ValidateChecksum(ref message))
            {
                Log.Warn(this, $"Invalid checksum for message - '{message}'.");
                SettingsParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            var match = _messageFormat.Match(message);
            if (!match.Success)
            {
                Log.Warn(this, $"Unable to parse settings message - '{message}'.");
                SettingsParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            var name = match.Groups["Name"].Value;
            var value = match.Groups["Value"].Value;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                SettingsParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            SettingsMessageReceived?.Invoke(this, new SettingMessageEventArgs(name, value));
        }

        protected override IDisposable Attach(IObservable<string> source, IObserver<string> sink)
        {
            return source.Where(message => message.StartsWith(MessagePrefix)).Subscribe(Process, OnCompleted);
        }

        private void OnCompleted()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler SettingsParseError;
        public event EventHandler<SettingMessageEventArgs> SettingsMessageReceived;
        public event EventHandler Disconnected;
    }
}