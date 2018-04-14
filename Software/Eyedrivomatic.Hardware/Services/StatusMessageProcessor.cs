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
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IStatusMessageSource))]
    [MessageProcessor("Status")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class StatusMessageProcessor : LineMessageProcessor, IStatusMessageSource
    {
        public const string MessagePrefix = "STATUS:";
        private readonly Regex _messageFormat = new Regex($@"^{MessagePrefix} SERVO_X=(?<XRelative>-?\d+)\((?<XAbsolute>-?\d{{1,3}}\.\d)\),SERVO_Y=(?<YRelative>-?\d+)\((?<YAbsolute>-?\d{{1,3}}\.\d)\),SWITCH 1=(?<Switch1>ON|OFF),SWITCH 2=(?<Switch2>ON|OFF),SWITCH 3=(?<Switch3>ON|OFF)$");

        internal void Process(string message)
        {
            if (!ChecksumProcessor.ValidateChecksum(ref message))
            {
                Log.Warn(this, $"Invalid checksum for message - '{message}'.");
                StatusParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            var match = _messageFormat.Match(message);
            if (!match.Success)
            {
                Log.Warn(this, $"Unable to parse status message - '{message}'.");
                StatusParseError?.Invoke(this, EventArgs.Empty);
                return;
            }

            StatusMessageReceived?.Invoke(this,
                new StatusMessageEventArgs(
                    Int32.Parse(match.Groups["XRelative"].Value, CultureInfo.InvariantCulture),
                    Double.Parse(match.Groups["XAbsolute"].Value, CultureInfo.InvariantCulture),
                    Int32.Parse(match.Groups["YRelative"].Value, CultureInfo.InvariantCulture),
                    Double.Parse(match.Groups["YAbsolute"].Value, CultureInfo.InvariantCulture), 
                    match.Groups["Switch1"].Value == "ON",
                    match.Groups["Switch2"].Value == "ON",
                    match.Groups["Switch3"].Value == "ON"));
        }

        protected override IDisposable Attach(IObservable<string> source, IObserver<string> sink)
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
