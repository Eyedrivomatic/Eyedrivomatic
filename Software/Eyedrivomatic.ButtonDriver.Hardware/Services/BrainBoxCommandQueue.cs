using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Logging;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IBrainBoxCommandQueue))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [MessageProcessor("Response")]
    internal class BrainBoxCommandQueue : IBrainBoxCommandQueue
    {
        internal const char Ack = (char)0x06;
        internal const char Nak = (char)0x15;

        private readonly Func<IBrainBoxCommand, IBrainBoxCommandHandler> _commandHandlerFactory;
        private BlockingCollection<IBrainBoxCommandHandler> _commandQueue = new BlockingCollection<IBrainBoxCommandHandler>();
        private CancellationTokenSource _currentCommandCts = new CancellationTokenSource();
        private volatile IBrainBoxCommandHandler _currentCommand;

        [Import("CommandTimeout")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(1000);

        [ImportingConstructor]
        internal BrainBoxCommandQueue([Import("BrainBoxCommandHandlerFactory")] Func<IBrainBoxCommand, IBrainBoxCommandHandler> commandHandlerFactory)
        {
            Contract.Requires<ArgumentNullException>(commandHandlerFactory != null, nameof(commandHandlerFactory));

            _commandHandlerFactory = commandHandlerFactory;
        }

        [Export("ExecuteCommand")]
        public Task<bool> SendCommand(IBrainBoxCommand command)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BrainBoxCommandQueue));

            var handler = _commandHandlerFactory(command);
            handler.StartTimeoutTimer(command.DefaultTimeout);
            _commandQueue.Add(handler, _currentCommandCts.Token);

            return handler.CommandTask;
        }

        public void Attach(IObservable<char> source)
        {
            _currentCommandCts = new CancellationTokenSource();
            _commandQueue = new BlockingCollection<IBrainBoxCommandHandler>();

            //Handle Ack/Nak.
            source.Where(c => c == Ack || c == Nak).Subscribe(HandleResponse, Disconected);

            Task.Run(ProcessCommands);
        }

        private void Disconected()
        {
            _commandQueue.CompleteAdding();
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
                ButtonDriverHardwareModule.Logger?.Log($"Unexpected [{(response == Ack ? "ACK" : "NAK")}] response received!", Category.Warn, Priority.None);
                return;
            }
            ButtonDriverHardwareModule.Logger?.Log($"Handling [{(response == Ack ? "ACK" : "NAK")}] response for [{cmd.Name}] command.", Category.Debug, Priority.None);

            try
            {
                if (!cmd.HandleResponse(response == Ack))
                {
                    ButtonDriverHardwareModule.Logger?.Log($"Failed to handle response for [{cmd.Name}] command.", Category.Warn, Priority.None);
                }
            }
            catch (Exception ex)
            {
                ButtonDriverHardwareModule.Logger?.Log($"Exception handling [{cmd.Name}] command - [{ex}].", Category.Exception, Priority.None);
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
