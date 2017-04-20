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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [ContractClass(typeof(Contracts.ButtonDriverConfigurationServiceContract))]
    public interface IButtonDriverConfigurationService : INotifyPropertyChanged
    {
        bool AutoConnect { get; set; }
        string ConnectionString { get; set; }

        bool AutoSaveDeviceSettingsOnExit { get; set; }

        bool SafetyBypass { get; set; }

        ObservableCollection<Profile> DrivingProfiles { get; }

        Profile CurrentProfile { get; set; }

        TimeSpan CommandTimeout { get; set; }

        void Save();
        bool HasChanges { get; }
    }


    namespace Contracts
    {
        [ContractClassFor(typeof(IButtonDriverConfigurationService))]
        internal abstract class ButtonDriverConfigurationServiceContract : IButtonDriverConfigurationService
        {
            public abstract event PropertyChangedEventHandler PropertyChanged;
            public abstract bool AutoConnect { get; set; }
            public abstract string ConnectionString { get; set; }
            public abstract bool AutoSaveDeviceSettingsOnExit { get; set; }
            public abstract bool SafetyBypass { get; set; }

            public ObservableCollection<Profile> DrivingProfiles
            {
                get
                {
                    Contract.Ensures(Contract.Result<ObservableCollection<Profile>>() != null);
                    return default(ObservableCollection<Profile>);
                }
            }

            public Profile CurrentProfile
            {
                get { return default(Profile); }
                set { Contract.Requires<ArgumentException>(value != null); }
            }

            public abstract TimeSpan CommandTimeout { get; set; }
            public abstract void Save();
            public abstract bool HasChanges { get; }
        }

}
}
