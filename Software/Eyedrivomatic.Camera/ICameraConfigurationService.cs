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


using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Accord.Video.DirectShow;

namespace Eyedrivomatic.Camera
{
    public interface ICameraConfigurationService : INotifyPropertyChanged
    {
        bool CameraEnabled { get; set; }
        FilterInfo Camera { get; set; }
        IEnumerable<FilterInfo> AvailableCameras { get; }
        double OverlayOpacity { get; set; }
        Stretch Stretch { get; set; }

        bool HasChanges { get; }
        void Save();
    }
}
