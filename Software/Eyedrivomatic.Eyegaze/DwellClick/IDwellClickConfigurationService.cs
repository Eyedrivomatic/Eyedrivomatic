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


using System.ComponentModel;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public enum DwellClickActivationRole
    {
        Standard,
        DirectionButtons,
        StopButton,
        StartButton
    }

    public interface IDwellClickConfigurationService : INotifyPropertyChanged
    {
        bool EnableDwellClick { get; set; }
        string Provider { get; set; }
        int StandardDwellTimeMilliseconds { get; set; }
        int DirectionButtonDwellTimeMilliseconds { get; set; }
        int StartButtonDwellTimeMilliseconds { get; set; }
        int StopButtonDwellTimeMilliseconds { get; set; }
        int DwellTimeoutMilliseconds { get; set; }
        int RepeatDelayMilliseconds { get; set; }

        void Save();
        bool HasChanges { get; }
    }
}
