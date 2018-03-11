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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Eyedrivomatic.Eyegaze.Interfaces.Dynavox.Interop;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    [ExportEyegazeProvider("Tobii")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class TobiiDynavoxEyegazeProvider : IEyegazeProvider
    {
        public class TobiiDynavoxWpfInteractorAgent : IDisposable
        {
            private readonly IDynavoxHost _host;
            private IDisposable _observerRegistration;

            private static readonly Dictionary<GazeData.TrackingStatus, Func<GazeData, Point?>> DataFilter = new Dictionary<GazeData.TrackingStatus, Func<GazeData, Point?>>
            {
                { GazeData.TrackingStatus.NoEyesTracked, data => null },
                { GazeData.TrackingStatus.OneEyeTrackedUnknownWhich, data => null },
                { GazeData.TrackingStatus.BothEyesTracked, data => new Point((data.GazePointLeft.X+data.GazePointRight.X)/2, (data.GazePointLeft.Y+data.GazePointRight.Y)/2) },
                { GazeData.TrackingStatus.OneEyeTrackedProbablyLeft, data => new Point(data.GazePointLeft.X, data.GazePointLeft.Y) },
                { GazeData.TrackingStatus.OneEyeTrackedProbablyRight, data => new Point(data.GazePointRight.X, data.GazePointRight.Y ) },
                { GazeData.TrackingStatus.OnlyLeftEyeTracked, data => new Point(data.GazePointLeft.X, data.GazePointRight.Y ) },
                { GazeData.TrackingStatus.OnlyRightEyeTracked, data => new Point(data.GazePointRight.X, data.GazePointRight.Y ) }
            };


            public TobiiDynavoxWpfInteractorAgent(IDynavoxHost host)
            {
                _host = host;
                _host.DataStream
                    .SubscribeOnDispatcher()
                    .Select(data =>
                    {
                        var point = DataFilter[data.Status](data);
                        var visual = VisualTreeHelper.HitTest()
                    new {, VisualTreeHelper.HitTest() )}}
            }

            private void OnCompleted()
            {
                throw new NotImplementedException();
            }

            private void OnNext(GazeData gazeData)
            {
                throw new NotImplementedException();
            }

            private void OnError(Exception error)
            {
                
            }

            public void Dispose()
            {
                _observerRegistration?.Dispose();
            }
        }


        private IDynavoxHost _host;

        public void Dispose()
        {
            _host?.Dispose();
            _host = null;
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
            return new TobiiDynavoxProviderRegistration(client, _host.DataStream);

        }
    }


}