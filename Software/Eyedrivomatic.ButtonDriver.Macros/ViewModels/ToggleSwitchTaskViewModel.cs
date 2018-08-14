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

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    public class CycleSwitchTaskViewModel : EditMacroTaskViewModel
    {
        private CycleSwitchTask CycleSwitchTask => (CycleSwitchTask)Task;
        private readonly uint _deviceRelayCount;

        public CycleSwitchTaskViewModel(CycleSwitchTask task, uint deviceRelayCount)
            : base(task)
        {
            _deviceRelayCount = deviceRelayCount;
        }

        public override string Description => Strings.CycleSwitchMacroTask_Description;

        public uint SwitchNumber
        {
            get => CycleSwitchTask.SwitchNumber;
            set
            {
                if (CycleSwitchTask.SwitchNumber == value) return;
                CycleSwitchTask.SwitchNumber = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Info));
            }
        }

        public uint Repeat
        {
            get => CycleSwitchTask.Repeat;
            set
            {
                if (CycleSwitchTask.Repeat == value) return;
                CycleSwitchTask.Repeat = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Info));
            }
        }

        public uint DelayMs
        {
            get => CycleSwitchTask.ToggleDelayMs;
            set
            {
                if (CycleSwitchTask.ToggleDelayMs == value) return;
                CycleSwitchTask.ToggleDelayMs = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Info));
            }
        }

        public IEnumerable<uint> Relays => new uint[_deviceRelayCount];
    }
}
