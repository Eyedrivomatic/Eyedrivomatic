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


using System.ComponentModel.Composition;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class StatusViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        [ImportingConstructor]
        public StatusViewModel(IHardwareService hardwareService)
            : base(hardwareService)
        {
        }

        public string HeaderInfo => Strings.ViewName_Status;

        public SafetyBypassState SafetyBypassStatus => HardwareService.CurrentDriver?.SafetyBypassStatus ?? SafetyBypassState.Safe;
        public bool DiagonalSpeedReduction => HardwareService.CurrentDriver?.DiagonalSpeedReduction ?? false;

        public Direction LastDirection => HardwareService.CurrentDriver?.LastDirection ?? Direction.None;
        public Direction JoystickState => HardwareService.CurrentDriver?.CurrentDirection ?? Direction.None;

        public Speed Speed => HardwareService.CurrentDriver?.Speed ?? Speed.None;

        public int XServoCenter => HardwareService.CurrentDriver?.XServoCenter ?? 90;
        public int YServoCenter => HardwareService.CurrentDriver?.YServoCenter ?? 90;
        public Speed NudgeSpeed => HardwareService.CurrentDriver?.NudgeSpeed ?? Speed.Slow;
        public double NudgeDuration => HardwareService.CurrentDriver?.NudgeDuration/1000d ?? 0;
        public double YDuration => HardwareService.CurrentDriver?.YDuration/1000d ?? 0;
        public double XDuration => HardwareService.CurrentDriver?.XDuration/1000d ?? 0;

        public ReadyState ReadyState => HardwareService.CurrentDriver?.ReadyState ?? ReadyState.None;
    }
}
