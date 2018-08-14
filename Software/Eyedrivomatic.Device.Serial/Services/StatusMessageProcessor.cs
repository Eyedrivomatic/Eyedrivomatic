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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Serial.Communications;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Device.Serial.Services
{
    [Export(typeof(IStatusMessageSource))]
    [MessageProcessor("Status")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class StatusMessageProcessor : LineMessageProcessor, IStatusMessageSource
    {
        public const string MessagePrefix = "STATUS:";
        private readonly Regex _messageFormat = new Regex($@"^{MessagePrefix}: [(POS=(?<Pos>-?\d{{1,3}}\.\d{{1,3}},-?\d{{1,3}}\.\d{{1,3}}))|(VECTOR=(?<Vector>-?\d{{1,3}}\.\d{{1,3}},-?\d{{1,3}}\.\d{{1,3}}))],(?<SwitchState>SWITCH (?<SwitchNumber>\d+)=(?<State>ON|OFF))+$");

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

            var switchStates = new List<bool>();
            for (var iSwitchState = 1; iSwitchState < match.Groups.Count; iSwitchState+=3)
            {
                switchStates[int.Parse(match.Groups[iSwitchState+1].Value)-1] = match.Groups[iSwitchState+2].Value == "ON";
            }

            if (match.Groups["Pos"].Success)
            {
                StatusMessageReceived?.Invoke(this,
                    new StatusMessageEventArgs(
                        Point.Parse(match.Groups["Pos"].Value),
                        switchStates.ToArray()));

            }
            else
            {
                StatusMessageReceived?.Invoke(this,
                    new StatusMessageEventArgs(
                        Vector.Parse(match.Groups["Vector"].Value),
                        switchStates.ToArray()));
            }
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
