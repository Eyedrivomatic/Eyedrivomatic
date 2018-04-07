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
using System.Linq;
using System.Threading.Tasks;
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
    public partial class TobiiEyegazeProvider : IEyegazeProvider
    {
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private Host _host;
        private WpfInteractorAgent _interactorAgent;
        private EngineStateObserver<EyeTrackingDeviceStatus> _deviceStatusObserver;
        private EngineStateObserver<UserPresence> _userPresenceObserver;
        private Environment _environment;

        public Task<bool> InitializeAsync()
        {
            try
            {
                Log.Info(this, "Tobii environment initializing.");
                if (Environment.GetEyeXAvailability() == EyeXAvailability.NotAvailable)
                {
                    Log.Error(this, "Tobii  EyeX Engine is not available.");
                }

                _environment = Environment.Initialize(LogWriter);
                if (!Environment.IsInitialized)
                {
                    Log.Error(this, "Failed to initialize the Tobii environment.");
                    return Task.FromResult(false);
                }

                _host = new Host();
                Log.Info(this, $"Tobii EyeX Availability: [{Host.EyeXAvailability}]");
                Log.Info(this, $"Tobii host connection state: [{_host.Context.ConnectionState}]");
                _host.Context.ConnectionStateChanged += (sender, args) => Log.Info(this, $"Tobii host connection state changed: [{args.State}]");
                _deviceStatusObserver = _host.States.CreateEyeTrackingDeviceStatusObserver();
                _deviceStatusObserver.WhenChanged(value => Log.Info(this, $"Tobii device status changed: [{value}]."));

                _userPresenceObserver = _host.States.CreateUserPresenceObserver();
                _userPresenceObserver.WhenChanged(value => Log.Info(this, $"Tobii user presence detection changed: [{value}]."));

                _interactorAgent = _host.InitializeWpfAgent();
                return Task.FromResult(Host.EyeXAvailability == EyeXAvailability.Running);
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to initialize the Tobii Eyegaze provider: [{ex}].");
                return Task.FromResult(false);
            }
        }

        private static readonly Dictionary<LogLevel, Action<string>> LogMapper = new Dictionary<LogLevel, Action<string>>
        {
            {LogLevel.Debug, msg => Log.Debug(typeof(Environment), msg)},
            {LogLevel.Info, msg => Log.Info(typeof(Environment), msg)},
            {LogLevel.Warning, msg => Log.Warn(typeof(Environment), msg)},
            {LogLevel.Error, msg => Log.Error(typeof(Environment), msg)},
        };

        private static void LogWriter(LogLevel level, string scope, string message)
        {
            if (LogMapper.TryGetValue(level, out Action<string> action)) action($"Tobii: {scope} - {message}");
            else Log.Error(typeof(Environment), $"Tobii: {scope} - {message}");
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            var interactor = _interactorAgent.AddInteractorFor(element);

            var registration = new TobiiProviderRegistration(interactor, client, r =>
            {
                _registrations.Remove(r);
                if (!_registrations.Any())
                {
                    Log.Info(this, "Disabling Tobii Connection");
                    _host.DisableConnection();
                }
            });

            if (!_registrations.Any())
            {
                Log.Info(this, "Enabling Tobii Connection");
                _host.EnableConnection();
            }
            _registrations.Add(registration);
            return registration;
        }

        public void Dispose()
        {
            Log.Info(this, "Disposing Tobii Eyegaze provider.");
            _interactorAgent?.Dispose();
            _userPresenceObserver?.Dispose();
            _deviceStatusObserver?.Dispose();
            _host?.Dispose();
            _environment?.Dispose();
        }
    }
}
