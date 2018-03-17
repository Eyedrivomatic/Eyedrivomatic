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
using System.Windows;
using System.Windows.Media;
using Eyedrivomatic.Eyegaze.Interfaces.Dynavox.Interop;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox.DynavoxInterop.TestApp
{
    /// <summary>
    ///     Interaction logic for GazePositionView.xaml
    /// </summary>
    public partial class GazePositionView : IObserver<GazeData>
    {
        private static readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IDynavoxHost _host;

        public GazePositionView()
        {
            Log.Debug("Created.");
            InitializeComponent();
            GazeStatus.Content = GazeData.TrackingStatus.NoEyesTracked.ToString();
        }

        protected override void OnInitialized(EventArgs e)
        {
            Log.Debug("Initialized.");
            base.OnInitialized(e);
            StartTobii();
            Dispatcher.ShutdownStarted += OnDispatcherShutDownStarted;

        }

        private void OnDispatcherShutDownStarted(object sender, EventArgs e)
        {
            Log.Debug("OnDispatcherShutDownStarted Invoked.");
            _host?.Dispose();
        }

        private async void StartTobii()
        {
            Log.Debug("Starting Tobii.");
            try
            {
                if (!DynavoxHostFactory.IsAvailable)
                {
                    Log.Debug("Tobii not detected.");
                    GazeStatus.Content = "Tobii Not Detected";
                    return;
                }
                Log.Debug("Creating Host.");
                _host = DynavoxHostFactory.CreateHost();
                Log.Debug("Host Created.");

                Log.Debug("Initializing Host.");
                await _host.InitializeAsync(LoggingCallback);
                Log.Debug("Host Initialized.");

                Log.Debug("Subscribing to data stream.");
                _host.DataStream.Subscribe(this);
                Log.Debug("Subscribed to data stream.");
            }
            catch (Exception exception)
            {
                LoggingCallback(LogLevel.Error, $"Failed to start eyegaze host - {exception}");
            }
        }

        private void LoggingCallback(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                    Log.Error(message);
                    break;
                case LogLevel.Warning:
                    Log.Warn(message);
                    break;
                case LogLevel.Information:
                    Log.Info(message);
                    break;
                case LogLevel.Diagnostic:
                    Log.Debug(message);
                    break;
            }

            LogView.Text += $"{logLevel}: {message}\n";
            LogScrollViewer.ScrollToBottom();
        }

        private static readonly Dictionary<GazeData.TrackingStatus, Func<GazeData, Point2D?>> DataFilter = new Dictionary<GazeData.TrackingStatus, Func<GazeData, Point2D?>>
        {
            { GazeData.TrackingStatus.NoEyesTracked, data => null },
            { GazeData.TrackingStatus.OneEyeTrackedUnknownWhich, data => null },
            { GazeData.TrackingStatus.BothEyesTracked, data => new Point2D((data.GazePointLeft.X+data.GazePointRight.X)/2, (data.GazePointLeft.Y+data.GazePointRight.Y)/2) },
            { GazeData.TrackingStatus.OneEyeTrackedProbablyLeft, data => new Point2D(data.GazePointLeft.X, data.GazePointLeft.Y) },
            { GazeData.TrackingStatus.OneEyeTrackedProbablyRight, data => new Point2D(data.GazePointRight.X, data.GazePointRight.Y ) },
            { GazeData.TrackingStatus.OnlyLeftEyeTracked, data => new Point2D(data.GazePointLeft.X, data.GazePointRight.Y ) },
            { GazeData.TrackingStatus.OnlyRightEyeTracked, data => new Point2D(data.GazePointRight.X, data.GazePointRight.Y ) }
        };

        public void OnNext(GazeData value)
        {
            LoggingCallback(LogLevel.Diagnostic, $"status={value.Status}, Left=({value.GazePointLeft.X}, {value.GazePointLeft.Y}), Right=({value.GazePointRight.X}, {value.GazePointRight.Y})");
            var point = DataFilter[value.Status](value);

            if (point == null)
            {
                Background = Brushes.White;
                GazeStatus.Content = "Gaze not detected.";
                GazeX.Content = string.Empty;
                GazeY.Content = string.Empty;
            }
            else
            {
                GazeStatus.Content = "Tracking";
                GazeX.Content = point.Value.X.ToString("0");
                GazeY.Content = point.Value.Y.ToString("0");

                var localPoint = PointFromScreen(new Point(point.Value.X, point.Value.Y));
                Background = new RadialGradientBrush(Colors.White, Colors.LightGray)
                {
                    GradientOrigin = localPoint,
                    Center = localPoint,
                    MappingMode = BrushMappingMode.Absolute,
                    RadiusX = 200,
                    RadiusY = 200
                };
            }
        }

        public void OnError(Exception error)
        {
            GazeStatus.Content = "ERROR";
            LoggingCallback(LogLevel.Error, error.ToString());
        }

        public void OnCompleted()
        {
            GazeStatus.Content = "Stopped";
            LoggingCallback(LogLevel.Information, "Gaze data stream stopped.");
        }
    }
}