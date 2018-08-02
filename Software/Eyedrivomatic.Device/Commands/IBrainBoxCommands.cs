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


using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;

namespace Eyedrivomatic.Device.Commands
{
    public interface IBrainBoxCommands
    {
        [Export(nameof(Move))]
        Task<bool> Move(Point position, TimeSpan duration);

        [Export(nameof(Go))]
        Task<bool> Go(double direction, double speed, TimeSpan duration);

        [Export(nameof(ToggleRelay))]
        Task<bool> ToggleRelay(uint relay, TimeSpan duration);

        [Export(nameof(GetStatus))]
        Task<bool> GetStatus();

        [Export(nameof(GetConfiguration))]
        Task<bool> GetConfiguration(string name);

        [Export(nameof(SetConfiguration))]
        Task<bool> SetConfiguration(string name, string value);

        [Export(nameof(SaveConfiguration))]
        Task<bool> SaveConfiguration();

        [Export(nameof(Stop))]
        Task<bool> Stop();
    }
}
