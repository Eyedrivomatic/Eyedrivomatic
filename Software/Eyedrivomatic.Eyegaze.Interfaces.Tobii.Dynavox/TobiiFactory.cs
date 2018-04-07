using System;
using System.ComponentModel.Composition;
using Tobii.Gaze.Core;
using Tobii.Gaze.Core.Internal;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    [Export(typeof(ITobiiFactory))]
    public class TobiiFactory : ITobiiFactory
    {
        public Func<IEyeTrackerCoreLibrary> CoreLibraryFactory => () => new EyeTrackerCoreLibrary();
        public Func<Uri, IEyeTracker> CreateEyeTracker => uri => new EyeTracker(uri);
        public IEyeTrackerCoreLibrary CreateLibrary()
        {
            return new EyeTrackerCoreLibrary();
        }

        IExtendedEyeTracker ITobiiFactory.CreateEyeTracker(Uri uri)
        {
            return new ExtendedEyeTracker(uri);
        }

        public DataStreamFilter CreateDataStream(IEyeTracker eyeTracker)
        {
            return new DataStreamFilter(eyeTracker);
        }
    }
}