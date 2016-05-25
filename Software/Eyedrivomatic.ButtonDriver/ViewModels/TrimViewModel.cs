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


using System.ComponentModel.Composition;
using System.Windows.Input;

using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class TrimViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        [ImportingConstructor]
        public TrimViewModel(IHardwareService hardwareService)
            : base(hardwareService)
        { }

        public string HeaderInfo => Strings.ViewName_Trim;

        public ICommand IncreaseNudgeSpeed => new DelegateCommand(() => HardwareService.CurrentDriver.IncreaseNudgeSpeed(), () => { return IsOnline && HardwareService.CurrentDriver.NudgeSpeed != Speed.Manic; });
        public ICommand DecreaseNudgeSpeed => new DelegateCommand(() => HardwareService.CurrentDriver.DecreaseNudgeSpeed(), () => { return IsOnline && HardwareService.CurrentDriver.NudgeSpeed != Speed.Slow; });

        public ICommand IncreaseNudgeDuration => new DelegateCommand(() => HardwareService.CurrentDriver.IncreaseNudgeDuration(), () => { return IsOnline && HardwareService.CurrentDriver.NudgeDuration < 3000; });
        public ICommand DecreaseNudgeDuration => new DelegateCommand(() => HardwareService.CurrentDriver.DecreaseNudgeDuration(), () => { return IsOnline && HardwareService.CurrentDriver.NudgeDuration > 0; });

        public ICommand TrimForward => new DelegateCommand(() => HardwareService.CurrentDriver.TrimForward(), () => { return IsOnline && HardwareService.CurrentDriver.YServoCenter < 120; });
        public ICommand TrimBackward => new DelegateCommand(() => HardwareService.CurrentDriver.TrimBackward(), () => { return IsOnline && HardwareService.CurrentDriver.YServoCenter > 60; });

        public ICommand TrimLeft=> new DelegateCommand(() => HardwareService.CurrentDriver.TrimLeft(), () => { return IsOnline && HardwareService.CurrentDriver.XServoCenter < 120; });
        public ICommand TrimRight=> new DelegateCommand(() => HardwareService.CurrentDriver.TrimRight(), () => { return IsOnline && HardwareService.CurrentDriver.XServoCenter > 60; });

        bool IsOnline => HardwareService.CurrentDriver?.HardwareReady ?? false;
    }
}
