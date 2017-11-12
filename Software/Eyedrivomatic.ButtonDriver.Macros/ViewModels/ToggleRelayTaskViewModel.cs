// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


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
            get => CycleRelayTask.DelayMs;
            set
            {
                if (CycleRelayTask.DelayMs == value) return;
                CycleRelayTask.DelayMs = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
            }
        }

        public IEnumerable<uint> Relays => Enumerable.Range(1, (int)_deviceRelayCount).Cast<uint>();
    }
}
