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

namespace Eyedrivomatic.Device.Commands
{
    [Export(typeof(IBrainBoxCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DeviceCommands : IBrainBoxCommands
    {
        private readonly Func<IDeviceCommand, Task<bool>> _executeCommand;

        [ImportingConstructor]
        internal DeviceCommands([Import("ExecuteCommand")] Func <IDeviceCommand, Task<bool>> executeCommand)
        {
            _executeCommand = executeCommand;
        }

        [Export(nameof(IBrainBoxCommands.Move))]
        public Task<bool> Move(Point position, TimeSpan duration)
        {
            var command = new MoveCommand(position, duration);
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.Go))]
        public Task<bool> Go(Vector vector, TimeSpan duration)
        {
            var command = new GoCommand(vector, duration);
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.ToggleRelay))]
        public Task<bool> ToggleRelay(uint relay, TimeSpan duration)
        {
            var command = new ToggleRelayCommand(relay, duration);
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.GetStatus))]
        public Task<bool> GetStatus()
        {
            var command = new GetStatusCommand();
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.GetConfiguration))]
        public Task<bool> GetConfiguration(string name)
        {
            var command = new GetConfigurationCommand(name);
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.SetConfiguration))]
        public Task<bool> SetConfiguration(string name, string value)
        {
            var command = new SetConfigurationCommand(name, value);
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.SaveConfiguration))]
        public Task<bool> SaveConfiguration()
        {
            var command = new SaveConfigurationCommand();
            return _executeCommand(command);
        }

        [Export(nameof(IBrainBoxCommands.Stop))]
        public Task<bool> Stop()
        {
            var command = new StopCommand();
            return _executeCommand(command);
        }
    }
}