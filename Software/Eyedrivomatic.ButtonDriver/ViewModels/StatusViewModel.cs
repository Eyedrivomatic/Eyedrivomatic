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
using System.Windows;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class StatusViewModel : ButtonDriverViewModelBase, IStatusViewModel
    {
        [ImportingConstructor]
        public StatusViewModel(IHardwareService hardwareService)
            : base(hardwareService)
        {
        }

        public ConnectionState ConnectionState => Driver?.Connection?.State ?? ConnectionState.Disconnected;
        public bool SafetyBypassStatus => Driver?.Profile?.SafetyBypass ?? false;
        public bool DiagonalSpeedReduction => Driver?.Profile.DiagonalSpeedReduction ?? false;

        public Direction LastDirection => Driver?.LastDirection ?? Direction.None;
        public Point JoystickPosition => Driver == null ? new Point() : new Point(Driver.DeviceStatus.XPosition, Driver.DeviceStatus.YPosition);

        public string Profile => Driver?.Profile.Name;

        public string Speed => Driver?.Profile.CurrentSpeed?.Name ?? Strings.StatusView_Speed_None;
        public ReadyState ReadyState => Driver?.ReadyState ?? ReadyState.None;
    }
}
