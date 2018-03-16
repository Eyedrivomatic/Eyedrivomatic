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
using System.Windows;
using Eyedrivomatic.Eyegaze.Interfaces.Dynavox.Interop;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    [ExportEyegazeProvider("Tobii")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class TobiiDynavoxEyegazeProvider : IEyegazeProvider
    {
        private IDynavoxHost _host;
        private readonly Func<IDynavoxHost, DataStreamFilter> _dataStreamFactory;
        private DataStreamFilter _dataStream;

        public TobiiDynavoxEyegazeProvider(Func<IDynavoxHost, DataStreamFilter> dataStreamFactory)
        {
            _dataStreamFactory = dataStreamFactory;
        }

        public bool Initialize()
        {
            try
            {
                Log.Info(this, "Tobii host initializing.");
                if (!DynavoxHostFactory.IsAvailable)
                    Log.Error(this, "Tobii device is not available.");
                _host = DynavoxHostFactory.CreateHost();

                return _host.Initialize(LoggingCallback);
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to initialize the Tobii Eyegaze provider: [{ex}].");
                return false;
            }
        }

        private static readonly Dictionary<LogLevel, Action<object, string>> LogHandlers = new Dictionary<LogLevel, Action<object, string>>
        {
            {LogLevel.Error, Log.Error},
            {LogLevel.Warning, Log.Warn},
            {LogLevel.Information, Log.Info},
            {LogLevel.Diagnostic, Log.Debug}
        };

        private void LoggingCallback(LogLevel logLevel, string message)
        {
            LogHandlers[logLevel]?.Invoke(this, message);
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            if (_dataStream == null) _dataStream = _dataStreamFactory(_host);
            return _dataStream.AddRegistration(element, client);
        }

        public void Dispose()
        {
            _host?.Dispose();
            _dataStream = null;
            _host = null;
        }
    }


}