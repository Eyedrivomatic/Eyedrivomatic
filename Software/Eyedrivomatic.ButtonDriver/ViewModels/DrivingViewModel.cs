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
using System.Windows.Input;

using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class DrivingViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        [ImportingConstructor]
        public DrivingViewModel(IHardwareService hardwareService)
            : base (hardwareService)
        {
        }

        public string HeaderInfo { get; } = Strings.ViewName_OutdoorDriving;

        public ICommand TogglePower => new DelegateCommand(async () => await HardwareService.CurrentDriver?.ToggleRelayAsync(1), () => { return IsOnline; });

        public ICommand SetXDuration => new DelegateCommand<string>(duration => { HardwareService.CurrentDriver.XDuration = ulong.Parse(duration); }, duration => { ulong tmp;  return ulong.TryParse(duration, out tmp) && tmp >= 0 && IsOnline; });
        public ICommand SetYDuration => new DelegateCommand<string>(duration => { HardwareService.CurrentDriver.YDuration = ulong.Parse(duration); }, duration => { ulong tmp; return ulong.TryParse(duration, out tmp) && tmp >= 0 && IsOnline; });
        public ICommand DiagonalSpeedReductionToggle => new DelegateCommand(() => { HardwareService.CurrentDriver.DiagonalSpeedReduction = !HardwareService.CurrentDriver.DiagonalSpeedReduction; }, ()=> IsOnline);

        public ICommand Continue => new DelegateCommand(() => HardwareService.CurrentDriver?.Continue(), () => IsOnline && (HardwareService.CurrentDriver.ReadyState == ReadyState.Any || HardwareService.CurrentDriver?.ReadyState == ReadyState.Continue));

        public ICommand Reset => new DelegateCommand(() => HardwareService.CurrentDriver?.Reset(), () => IsOnline);

        public ICommand Nudge => new DelegateCommand<XDirection?>(direction => HardwareService.CurrentDriver?.Nudge(direction.Value), direction => direction.HasValue && IsOnline);
        public ICommand Move => new DelegateCommand<Direction?>(direction => HardwareService.CurrentDriver?.Move(direction.Value), direction => direction.HasValue && IsOnline && HardwareService.CurrentDriver.CanMove(direction.Value));

        public ICommand SetSpeed => new DelegateCommand<Speed?>(speed => HardwareService.CurrentDriver.Speed = speed.Value, speed => speed.HasValue && IsOnline );

        public bool DiagnalSpeedReduction => IsOnline && HardwareService.CurrentDriver.DiagonalSpeedReduction;

        bool IsOnline => HardwareService.CurrentDriver?.HardwareReady ?? false;
    }

}
