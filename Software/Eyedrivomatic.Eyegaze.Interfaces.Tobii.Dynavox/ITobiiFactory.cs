using System;
using Tobii.Gaze.Core;
using Tobii.Gaze.Core.Internal;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    public interface ITobiiFactory
    {
        IEyeTrackerCoreLibrary CreateLibrary();
        IExtendedEyeTracker CreateEyeTracker(Uri uri);
        DataStreamFilter CreateDataStream(IEyeTracker eyeTracker);
    }
}