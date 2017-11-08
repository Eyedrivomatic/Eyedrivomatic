using System;
using System.ComponentModel.Composition;
using System.Windows;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Interaction.Wpf;
using Eyedrivomatic.Logging;
using Tobii.Interaction.Client;
using Environment = Tobii.Interaction.Model.Environment;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii
{
    [ExportEyegazeProvider("Tobii"), PartCreationPolicy(CreationPolicy.Shared)]
    public class TobiiEyegazeProvider : IEyegazeProvider
    {
        private readonly Host _host;
        private readonly WpfInteractorAgent _interactorAgent;
        private readonly EngineStateObserver<EyeTrackingDeviceStatus> _deviceStatusObserver;
        private readonly EngineStateObserver<UserPresence> _userPresenceObserver;
        private readonly Environment _environment;

        public TobiiEyegazeProvider()
        {
            if (!Environment.IsInitialized)
            {
                Log.Info(this, "Tobii environment initializing.");
                _environment = Environment.Initialize(LogWriter);
            }
            else
            {
                Log.Info(this, "Tobii environment already initialized.");
                _environment = null;
            }

            _host = new Host();
            Log.Info(this, $"Tobii EyeX Availability: [{Host.EyeXAvailability}]");
            Log.Info(this, $"Tobii host connection state: [{_host.Context.ConnectionState}]");
            _host.Context.ConnectionStateChanged += (sender, args) => Log.Info(this, $"Tobii host connection state changed: [{args.State}]");
            _deviceStatusObserver = _host.States.CreateEyeTrackingDeviceStatusObserver();
            _deviceStatusObserver.WhenChanged(value => Log.Info(this, $"Tobii device status changed: [{value}].") );

            _userPresenceObserver = _host.States.CreateUserPresenceObserver();
            _userPresenceObserver.WhenChanged(value => Log.Info(this, $"Tobii user presence detection changed: [{value}]."));

            _interactorAgent = _host.InitializeWpfAgent();
        }

        private void LogWriter(LogLevel level, string scope, string message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Log.Debug(this, $"Tobii: {scope} - {message}");
                    break;
                case LogLevel.Info:
                    Log.Info(this, $"Tobii: {scope} - {message}");
                    break;
                case LogLevel.Warning:
                    Log.Warn(this, $"Tobii: {scope} - {message}");
                    break;
                case LogLevel.Error:
                    Log.Error(this, $"Tobii: {scope} - {message}");
                    break;
                default:
                    Log.Warn(this, $"Tobii: {scope} - {message}");
                    break;
            }
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            var interactor = _interactorAgent.AddInteractorFor(element);
            return new TobiiProviderRegistration(interactor, client);
        }

        public void Dispose()
        {
            Log.Info(this, "Disposing Tobii Eyegaze provider.");
            _interactorAgent.Dispose();
            _userPresenceObserver.Dispose();
            _deviceStatusObserver.Dispose();
            _host.Dispose();
            _environment?.Dispose();
        }
    }
}
