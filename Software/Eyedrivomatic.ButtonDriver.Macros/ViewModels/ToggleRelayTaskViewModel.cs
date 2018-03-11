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
using System.Linq;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    public class CycleRelayTaskViewModel : EditMacroTaskViewModel
    {
        private CycleRelayTask CycleRelayTask => (CycleRelayTask)base.Task;
        private readonly uint _deviceRelayCount;

        public CycleRelayTaskViewModel(CycleRelayTask task, uint deviceRelayCount)
            : base(task)
        {
            _deviceRelayCount = deviceRelayCount;
        }

        public override string Description => Strings.CycleRelayMacroTask_Description;

        public uint Relay
        {
            get => CycleRelayTask.Relay;
            set
            {
                if (CycleRelayTask.Relay == value) return;
                CycleRelayTask.Relay = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
            }
        }

        public uint Repeat
        {
            get => CycleRelayTask.Repeat;
            set
            {
                if (CycleRelayTask.Repeat == value) return;
                CycleRelayTask.Repeat = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
            }
        }

        public uint DelayMs
        {
            get => CycleRelayTask.ToggleDelayMs;
            set
            {
                if (CycleRelayTask.ToggleDelayMs == value) return;
                CycleRelayTask.ToggleDelayMs = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
            }
        }

        public IEnumerable<uint> Relays => Enumerable.Range(1, (int)_deviceRelayCount).Cast<uint>();
    }
}
