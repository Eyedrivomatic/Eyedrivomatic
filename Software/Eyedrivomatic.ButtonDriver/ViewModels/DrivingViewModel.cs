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


using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

using Prism.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros.Models;
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

        public ICommand SetXDuration => new DelegateCommand<string>(
            duration =>
            {
                Driver.Profile.XDuration = TimeSpan.FromMilliseconds(ulong.Parse(duration));
            }, 
            duration => { ulong tmp;  return ulong.TryParse(duration, out tmp) && IsOnline; });

        public ICommand SetYDuration => new DelegateCommand<string>(
            duration =>
            {
                Driver.Profile.YDuration = TimeSpan.FromMilliseconds(ulong.Parse(duration));
            }, 
            duration => { ulong tmp; return ulong.TryParse(duration, out tmp) && IsOnline; });

        public ICommand DiagonalSpeedReductionToggle => new DelegateCommand(
            () => Driver.Profile.DiagonalSpeedReduction = !Driver.Profile.DiagonalSpeedReduction, 
            () => IsOnline);

        public ICommand Continue => new DelegateCommand(
            () => Driver.Continue(), 
            () => IsOnline && (Driver.ReadyState == ReadyState.Any || Driver.ReadyState == ReadyState.Continue));

        public ICommand Reset => new DelegateCommand(
            () => Driver.Stop(), 
            () => IsOnline);

        public ICommand Nudge => new DelegateCommand<XDirection?>(
            direction => { if (direction != null) Driver.Nudge(direction: direction.Value); }, 
            direction => direction.HasValue && IsOnline);
        public ICommand Move => new DelegateCommand<Direction?>(
            direction => { if (direction != null) Driver.Move(direction: direction.Value); }, 
            direction => direction.HasValue && IsOnline && Driver.CanMove(direction.Value));

        public ICommand SetSpeed => new DelegateCommand<string>(
            speed =>
            {
                Driver.Profile.CurrentSpeed =
                    Driver.Profile.Speeds.Single(
                        s => string.Compare(s.Name, speed, StringComparison.CurrentCultureIgnoreCase) == 0);

            }, 
            speed =>
            {
                return !string.IsNullOrWhiteSpace(speed) && IsOnline &&
                       Driver.Profile.Speeds.Any(
                           s => string.Compare(s.Name, speed, StringComparison.CurrentCultureIgnoreCase) == 0);
            });

        [Import("ExecuteMacroCommand")]
        public ICommand ExecuteMacroCommand { get; internal set; }

        [Import("DrivingPageMacro")]
        public IMacro DrivingPageMacro { get; internal set; }

        public bool DiagnalSpeedReduction => IsOnline && Driver.Profile.DiagonalSpeedReduction;

        public bool IsOnline => Driver?.HardwareReady ?? false;
    }

}
