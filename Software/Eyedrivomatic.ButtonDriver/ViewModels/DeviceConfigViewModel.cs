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


using System.Collections.Generic;
using System.Windows.Input;

using Eyedrivomatic.ButtonDriver.Hardware;
using Prism.Commands;
using Eyedrivomatic.Controls;
using System;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public abstract class DeviceConfigViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        public DeviceConfigViewModel(IHardwareService hardwareService)
            : base(hardwareService)
        {
        }

        public string HeaderInfo => Strings.ViewName_Setup;

        public abstract string SelectedDevice { get; set; }
        public abstract ICommand Connect { get; }
        public abstract ICommand Disconnect { get; }
        public abstract ICommand ApplyChanges { get; }

        public virtual bool Ready => HardwareService.CurrentDriver?.HardwareReady ?? false;

        public virtual IList<string> AvailableDevices => HardwareService.CurrentDriver?.GetAvailableDevices();

        public virtual ICommand RefreshAvailableDeviceList => new DelegateCommand(
            () =>
            {
                HardwareService.CurrentDriver?.GetAvailableDevices();
                OnPropertyChanged(nameof(AvailableDevices));
            },
            () => true);
    }
}
