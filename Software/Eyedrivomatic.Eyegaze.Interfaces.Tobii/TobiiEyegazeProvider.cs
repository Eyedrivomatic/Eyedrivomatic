﻿using System;
using System.Collections.Generic;
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
    public partial class TobiiEyegazeProvider : IEyegazeProvider
    {
        private Host _host;
        private WpfInteractorAgent _interactorAgent;
        private EngineStateObserver<EyeTrackingDeviceStatus> _deviceStatusObserver;
        private EngineStateObserver<UserPresence> _userPresenceObserver;
        private Environment _environment;

        public bool Initialize()
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
                    return false;
                }

                _host = new Host();
                _host.EnableConnection();
                Log.Info(this, $"Tobii EyeX Availability: [{Host.EyeXAvailability}]");
                Log.Info(this, $"Tobii host connection state: [{_host.Context.ConnectionState}]");
                _host.Context.ConnectionStateChanged += (sender, args) => Log.Info(this, $"Tobii host connection state changed: [{args.State}]");
                _deviceStatusObserver = _host.States.CreateEyeTrackingDeviceStatusObserver();
                _deviceStatusObserver.WhenChanged(value => Log.Info(this, $"Tobii device status changed: [{value}]."));

                _userPresenceObserver = _host.States.CreateUserPresenceObserver();
                _userPresenceObserver.WhenChanged(value => Log.Info(this, $"Tobii user presence detection changed: [{value}]."));

                _interactorAgent = _host.InitializeWpfAgent();
                return Host.EyeXAvailability == EyeXAvailability.Running;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to initialize the Tobii Eyegaze provider: [{ex}].");
                return false;
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
            return new TobiiProviderRegistration(interactor, client);
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
