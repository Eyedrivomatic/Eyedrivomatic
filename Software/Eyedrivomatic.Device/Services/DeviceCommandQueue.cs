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
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Device.Services
{
    [Export(typeof(IDeviceCommandQueue))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [MessageProcessor("Response")]
    internal class DeviceCommandQueue : IDeviceCommandQueue
    {
        internal const char Ack = (char)0x06;
        internal const char Nak = (char)0x15;

        private readonly Func<IDeviceCommand, Action<string>, IDeviceCommandHandler> _commandHandlerFactory;
        private BlockingCollection<IDeviceCommandHandler> _commandQueue = new BlockingCollection<IDeviceCommandHandler>();
        private CancellationTokenSource _currentCommandCts = new CancellationTokenSource();
        private volatile IDeviceCommandHandler _currentCommand;
        private IObserver<string> _commandSink;

        [Import("CommandTimeout")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(1000);

        [ImportingConstructor]
        internal DeviceCommandQueue([Import("DeviceCommandHandlerFactory")] Func<IDeviceCommand, Action<string>, IDeviceCommandHandler> commandHandlerFactory)
        {
            _commandHandlerFactory = commandHandlerFactory;
        }

        [Export("ExecuteCommand")]
        public Task<bool> SendCommand(IDeviceCommand command)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DeviceCommandQueue));
            if (_commandSink == null)
            {
                Log.Warn(this, $"Failed to send [{command.Name}]. Device not connected.");
                return Task.FromResult(false);
            }

            var handler = _commandHandlerFactory(command, _commandSink.OnNext);
            if (command.DefaultTimeout > TimeSpan.Zero)
            {
                Log.Debug(this, $"Command [{command.Name}] will timeout after [{command.DefaultTimeout}]");
                handler.StartTimeoutTimer(command.DefaultTimeout);
            }
            _commandQueue.Add(handler, _currentCommandCts.Token);

            return handler.CommandTask;
        }

        public void Attach(IObservable<char> source, IObserver<string> sink)
        {
            _currentCommandCts = new CancellationTokenSource();
            _commandQueue = new BlockingCollection<IDeviceCommandHandler>();
            _commandSink = sink;

            //Handle Ack/Nak.
            source.Where(c => c == Ack || c == Nak).Subscribe(HandleResponse, Disconected);

            Task.Run(ProcessCommands);
        }

        private void Disconected()
        {
            _commandQueue.CompleteAdding();
            _commandSink = null; //This does not own _commandSink, so don't send OnComplete.
        }

        private async Task ProcessCommands()
        {
            _currentCommand = null;
            var queue = _commandQueue;
            var commandCts = _currentCommandCts;

            foreach (var command in queue.GetConsumingEnumerable())
            {
                _currentCommand = command;

                //If the send command fails, this retries the command until the command has decided that it should give up.
                while (!_currentCommand.CommandTask.IsCompleted)
                {
                    await _currentCommand.Send(Timeout, commandCts.Token);
                }
            }

            //Any currently running commands should be canceled. They will never receive a response.
            commandCts?.Cancel();
            queue.Dispose();
        }

        private void HandleResponse(char response)
        {
            var cmd = _currentCommand;
            if (cmd == null)
            {
                Log.Warn(this, $"Unexpected [{(response == Ack ? "ACK" : "NAK")}] response received!");
                return;
            }
            Log.Debug(this, $"Handling [{(response == Ack ? "ACK" : "NAK")}] response for [{cmd.Name}] command.");

            try
            {
                if (!cmd.HandleResponse(response == Ack))
                {
                    Log.Warn(this, $"Failed to handle response for [{cmd.Name}] command.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Exception handling [{cmd.Name}] command - [{ex}].");
            }
        }

        #region IDisposable
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _currentCommandCts.Cancel();
            _commandQueue.CompleteAdding();
        }
        #endregion //IDisposable
    }
}
