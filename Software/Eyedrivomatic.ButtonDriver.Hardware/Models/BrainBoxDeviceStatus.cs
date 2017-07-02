using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Hardware.Models
{
    [Export(typeof(IDeviceStatus))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class BrainBoxDeviceStatus : BindableBase, IDeviceStatus
    {
        private readonly IStatusMessageSource _statusMessageSource;
        private readonly Func<Task<bool>> _getStatusCommand;

        [ImportingConstructor]
        internal BrainBoxDeviceStatus(IStatusMessageSource statusMessageSource, [Import(nameof(IBrainBoxCommands.GetStatus))] Func<Task<bool>> getStatusCommand)
        {
            _statusMessageSource = statusMessageSource;
            _getStatusCommand = getStatusCommand;
            _statusMessageSource.StatusMessageReceived += OnStatusMessageReceived;
            _statusMessageSource.StatusParseError += OnStatusParseError;
            _statusMessageSource.Disconnected += OnDisconnected;
        }

        public bool IsKnown { get; private set; }

        public int XPosition { get; private set; }

        public int YPosition { get; private set; }

        public bool Switch1 { get; private set; }

        public bool Switch2 { get; private set; }

        public bool Switch3 { get; private set; }

        internal TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        private void OnStatusMessageReceived(object sender, StatusMessageEventArgs statusMessageEventArgs)
        {
            //Set all, then send a propery changed event.
            //This prevents the "double" updates on bound members.
            XPosition = statusMessageEventArgs.XRelative;
            YPosition = statusMessageEventArgs.YRelative;
            Switch1 = statusMessageEventArgs.Switch1;
            Switch2 = statusMessageEventArgs.Switch2;
            Switch3 = statusMessageEventArgs.Switch3;
            IsKnown = true;

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        private async void OnStatusParseError(object sender, EventArgs eventArgs)
        {
            Log.Error(this, $"Status error. Current status was [{(IsKnown ? "Known" : "Unknown")}.");

            //Don't "spam" the device. But ask the device for status right away if this is just a glitch.
            if (!IsKnown && RetryDelay > TimeSpan.Zero) await Task.Delay(RetryDelay);
            IsKnown = false;

            while (!await _getStatusCommand())
            {
                Log.Error(this, $"Status error. Failed to send 'get' status request.");
                if (RetryDelay > TimeSpan.Zero) await Task.Delay(RetryDelay);
            }
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            IsKnown = false;
            RaisePropertyChanged(nameof(IsKnown));
        }

        public void Dispose()
        {
            _statusMessageSource.StatusMessageReceived -= OnStatusMessageReceived;
            _statusMessageSource.StatusParseError -= OnStatusParseError;

            IsKnown = false;
            RaisePropertyChanged(nameof(IsKnown));
        }
    }
}
