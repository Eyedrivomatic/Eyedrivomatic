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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using Tobii.Gaze.Core;
using Eyedrivomatic.Logging;
using Tobii.Gaze.Core.Internal;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    //public static 

    [ExportEyegazeProvider("Tobii Dynavox")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TobiiDynavoxEyegazeProvider : IEyegazeProvider
    {
        private readonly IEyeTrackerCoreLibrary _library;
        private IExtendedEyeTracker _eyeTracker;
        private TaskCompletionSource<bool> _initializingTask; 

        private readonly ITobiiFactory _tobiiFactory;
        private DataStreamFilter _dataStream;

        [ImportingConstructor]
        public TobiiDynavoxEyegazeProvider(ITobiiFactory tobiiFactory)
        {
            _tobiiFactory = tobiiFactory;
            _library = CreateLibrary(_tobiiFactory.CreateLibrary);
        }

        private static IEyeTrackerCoreLibrary CreateLibrary(Func<IEyeTrackerCoreLibrary> factory)
        {
            try
            {
                var library = factory();
                Log.Info(nameof(TobiiDynavoxEyegazeProvider), $"Tobii library version [{library.LibraryVersion()}].");
                library.SetLogging("TobiiLog.txt", LogLevel.Info);
                return library;
            }
            catch (Exception exception)
            {
                Log.Error(nameof(TobiiDynavoxEyegazeProvider), $"Failed to create Eyetracker library - [{exception}]");
                return null;
            }
        }

        public Task<bool> InitializeAsync()
        {
            if (_initializingTask != null) return _initializingTask.Task;
            _initializingTask = new TaskCompletionSource<bool>();

            if (_library == null)
            {
                Log.Warn(this, "Unable to start Tobii. library unavailable.");
                _initializingTask.TrySetResult(false);
                return _initializingTask.Task;
            }

            Log.Info(this, "Starting Tobii.");
            try
            {
                var uri = _library.GetConnectedEyeTracker();
                if (uri == null)
                {
                    Log.Debug(this, "Tobii not detected.");
                    _initializingTask.TrySetResult(false);
                    return _initializingTask.Task;
                }

                Log.Debug(this, $"Tracker found at [{uri}].");

                _eyeTracker = _tobiiFactory.CreateEyeTracker(uri);
                _eyeTracker.EyeTrackerError += (sender, e) => Log.Error(this, $"The Tobii tracker encountered an error - [{e.ErrorCode}: {e.Message}].");
                Log.Debug(this, "Connecting.");

                _eyeTracker.RunEventLoopOnInternalThread(code =>
                {
                    if (code == ErrorCode.Success) Log.Info(this, "Tobii eye tracking thread terminated successfully.");
                    else
                    {
                        Log.Warn(this, $"Tobii eye tracking thread terminated abmormally - [{code}]");
                        _initializingTask.TrySetResult(false); //if still initializing.
                    }
                });

                _eyeTracker.ConnectAsync(code =>
                {
                    Log.Info(this, "Connected.");
                    _initializingTask.TrySetResult(code == ErrorCode.Success);
                });
                return _initializingTask.Task;
            }
            catch (Exception exception)
            {
                Log.Error(this, $"Failed to start eyegaze host [{exception}]");
                _initializingTask.TrySetResult(false);
                return _initializingTask.Task;
            }
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            if (_dataStream == null) _dataStream = _tobiiFactory.CreateDataStream(_eyeTracker);
            return _dataStream.AddRegistration(element, client);
        }

        public void Dispose()
        {
            _eyeTracker?.Dispose();
            _dataStream?.Dispose();
            _dataStream = null;
            _eyeTracker = null;
        }
    }


}