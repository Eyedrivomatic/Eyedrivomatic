using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    [Export(typeof(IBrainBoxCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class BrainBoxCommands : IBrainBoxCommands
    {
        private readonly Func<IBrainBoxCommand, Task<bool>> _executeCommand;

        [ImportingConstructor]
        internal BrainBoxCommands([Import("ExecuteCommand")] Func <IBrainBoxCommand, Task<bool>> executeCommand)
        {
            _executeCommand = executeCommand;
        }

        [Export(nameof(IBrainBoxCommands.Move))]
        public Task<bool> Move(int xPositionRelative, int yPositionRelative, TimeSpan duration)
        {
            var command = new MoveCommand(xPositionRelative, yPositionRelative, duration);
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