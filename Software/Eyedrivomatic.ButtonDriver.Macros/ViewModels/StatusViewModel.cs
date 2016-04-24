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
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics.Contracts;

using Prism.Mvvm;

using Eyedrivomatic.Hardware;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Modules.Status.ViewModels
{
    public class StatusViewModel : BindableBase
    {
        private readonly IHardwareService _hardware;

        public StatusViewModel(IHardwareService hardware)
        {
            Contract.Requires<ArgumentNullException>(hardware != null, nameof(hardware));
            _hardware = hardware;
        }

        public string DisplayName { get { return "Status"; } }

        public SafetyBypassState SafetyBypassStatus => _hardware.CurrentDriver?.SafetyBypassStatus ?? SafetyBypassState.Safe;
        public bool DiagonalSpeedReduction => _hardware.CurrentDriver?.DiagonalSpeedReduction ?? false;

        public Direction LastDirection => _hardware.CurrentDriver?.LastDirection ?? Direction.None;
        public Direction JoystickState => _hardware.CurrentDriver?.CurrentDirection ?? Direction.None;

        public Speed Speed => _hardware.CurrentDriver?.Speed ?? Speed.None;

        public int XServoCenter => _hardware.CurrentDriver?.XServoCenter ?? 90;
        public int YServoCenter => _hardware.CurrentDriver?.YServoCenter ?? 90;
        public Speed NudgeSpeed => _hardware.CurrentDriver?.NudgeSpeed ?? Speed.Slow;
        public double NudgeDuration => _hardware.CurrentDriver?.NudgeDuration/1000d ?? 0;
        public double ForwardBackwardtDuration => _hardware.CurrentDriver?.ForwardBackwardDuration/1000d ?? 0;
        public double LeftRightDuration => _hardware.CurrentDriver?.LeftRightDuration/1000d ?? 0;

        public ReadyState ReadyState => _hardware.CurrentDriver?.ReadyState ?? ReadyState.None;
    }
}
