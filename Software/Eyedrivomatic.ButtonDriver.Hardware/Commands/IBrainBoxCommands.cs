using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    public interface IBrainBoxCommands
    {
        [Export(nameof(Move))]
        Task<bool> Move(int xPositionRelative, int yPositionRelative, TimeSpan duration);

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
