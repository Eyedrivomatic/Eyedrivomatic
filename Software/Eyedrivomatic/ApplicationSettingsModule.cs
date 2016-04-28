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
using System.ComponentModel.Composition.Hosting;

using Prism.Events;
using Prism.Mef.Modularity;
using Prism.Modularity;

using Eyedrivomatic.Infrastructure.Events;
using Eyedrivomatic.Properties;


namespace Eyedrivomatic
{
    [ModuleExport(typeof(ApplicationSettingsModule), InitializationMode = InitializationMode.WhenAvailable)]
    public class ApplicationSettingsModule : IModule
    {

        [Import]
        IEventAggregator EventAggregator { get; set; }

        [Export("AutoConnect")]
        public bool AutoConnect
        {
            get { return Settings.Default.AutoConnect; }
            set { Settings.Default.AutoConnect = value; }
        }

        [Export("DeviceConnectionString")]
        public string DeviceConnectionString
        {
            get { return Settings.Default.BrainBoxConnection; }
            set { Settings.Default.BrainBoxConnection = value; }
        }

        public void Initialize()
        {
            EventAggregator.GetEvent<SaveAutoConnectEvent>().Subscribe(SaveAutoConnect);
            EventAggregator.GetEvent<SaveDeviceConnectionStringEvent>().Subscribe(SaveDeviceConnectionString);
        }

        private void SaveAutoConnect(bool autoConnect)
        {
            Settings.Default.AutoConnect = autoConnect;
            Settings.Default.Save();
        }

        private void SaveDeviceConnectionString(string deviceConnectionString)
        {
            Settings.Default.BrainBoxConnection = deviceConnectionString;
            Settings.Default.Save();
        }
    }
}
