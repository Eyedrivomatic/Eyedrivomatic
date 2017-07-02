using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    [Export]
    internal class BrainBoxCommandHandlerFactory
    {
        private readonly IBrainBoxConnection _connection;

        [ImportingConstructor]
        internal BrainBoxCommandHandlerFactory(IBrainBoxConnection connection)
        {
            _connection = connection;
        }

        [Export("BrainBoxCommandHandlerFactory")]
        public virtual IBrainBoxCommandHandler CreateBrainBoxCommandHandler(IBrainBoxCommand command)
        { 
            return new BrainBoxCommandHandler(s =>_connection.SendMessage(ChecksumProcessor.ApplyChecksum(s)), command);
        }
        
    }

    internal class BrainBoxCommandHandler : IBrainBoxCommandHandler
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _sendTaskCompletionSource;
        private readonly CancellationTokenSource _timeoutCts = new CancellationTokenSource();
        private readonly IBrainBoxCommand _command;
        private readonly Action<string> _sendFunc;

        internal BrainBoxCommandHandler(Action<string> sendFunc, IBrainBoxCommand command)
        {
            _sendFunc = sendFunc;
            _command = command;
        }

        protected int SendAttempt;

        public string Name => _command.Name;

        public Task<bool> CommandTask => _taskCompletionSource.Task;

        public async Task Send(TimeSpan timeout, CancellationToken cancellationToken)
        {
            SendAttempt++;

            Log.Debug(this, $"Sending [{_command.Name}] command for attempt [{SendAttempt}].");

            var timeoutCts = new CancellationTokenSource(timeout);
            _sendTaskCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => OnCanceled(true)))
            using (timeoutCts.Token.Register(() => OnSendTimeout(timeout)))
            {
                _sendFunc(_command.ToString());
                await _sendTaskCompletionSource.Task;
            }
        }

        public void StartTimeoutTimer(TimeSpan timeout)
        {
            _timeoutCts.CancelAfter(timeout);
            _timeoutCts.Token.Register(() => OnCommandTimeout(timeout));
        }

        private void OnCommandTimeout(TimeSpan timeout)
        {
            if (!_taskCompletionSource.TrySetException(new TimeoutException($"The {_command.Name} command has timed out."))) return;

            Log.Error(this, $"The [{_command.Name}] command has timed out after [{timeout}].");
        }

        public bool HandleResponse(bool success)
        {
            if (_sendTaskCompletionSource == null)
            {
                Log.Error(this, $"Response received for unsent [{_command.Name}] command.");
                return false;
            }

            if (success)
            {
                Log.Debug(this, $"Success response recieved for [{_command.Name}] command.");
                _taskCompletionSource.TrySetResult(true);
            }
            else
            {
                Log.Warn(this, $"Fail response received for [{_command.Name}] command.");
                if (SendAttempt > _command.Retries)
                {
                    Log.Error(this, $"Failed final attempt to send the [{_command.Name}] command.");
                    var ex = new BrainBoxCommandException(_command, $"Failed to send the [{_command.Name}] command after [{SendAttempt}] tries.");
                    _taskCompletionSource.TrySetException(ex);
                }

            }

            if (!_sendTaskCompletionSource.TrySetResult(success))
            {
                Log.Error(this, $"Response received for completed [{_command.Name}] command.");
                return false;
            }
            return true;
        }

        public void OnError(string message)
        {
            Log.Error(this, $"The [{_command.Name}] command has failed - [{message}].");

            var ex = new BrainBoxCommandException(_command, message);
            _taskCompletionSource.TrySetException(ex);
            _sendTaskCompletionSource?.TrySetResult(false);
        }

        protected void OnSendTimeout(TimeSpan timeout)
        {
            Log.Error(this, $"The [{_command.Name}] command has timed out while waiting for a response after [{timeout}] on attept [{SendAttempt}].");

            if (SendAttempt > _command.Retries)
            {
                Log.Error(this, $"Failed final attempt to send the [{_command.Name}] command.");
                _taskCompletionSource.TrySetException(new BrainBoxCommandException(_command, $"Failed to send the [{_command.Name}] command after [{SendAttempt}] tries."));
            }

            _sendTaskCompletionSource?.TrySetResult(false);
        }

        protected void OnCanceled(bool cancelSend)
        {
            Log.Error(this, $"The [{_command.Name}] command was canceled on attempt [{SendAttempt}].");

            _taskCompletionSource.TrySetCanceled();
            if (cancelSend) _sendTaskCompletionSource.TrySetResult(false);
        }
    }
}