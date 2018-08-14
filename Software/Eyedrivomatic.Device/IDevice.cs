using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.Device
{
    public interface IDevice : INotifyPropertyChanged
    {
        #region Connection
        IDeviceConnection Connection { get; }
        #endregion Connection

        /// <summary>
        /// The device is currently sending valid status messages.
        /// </summary>
        bool DeviceReady { get; }

        /// <summary>
        /// Settings that are managed by and saved on the device.
        /// These values are not valid until the device has connected and reported its settings.
        /// </summary>
        IDeviceSettings DeviceSettings { get; }

        /// <summary>
        /// The current positions of the servos and state of the relays. 
        /// These values are not valid until the device has connected and reported it status.
        /// </summary>
        IDeviceStatus DeviceStatus { get; }

        /// <summary>
        /// Toggle the specefied switch. Repeat as desired.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">If the relay does not exist.</exception>
        /// <param name="relay">The relay to cycle.</param>
        /// <param name="repeat">The number of times to repeat the cycle. If 0, the relay will not be activated.</param>
        /// <param name="toggleDelayMs">The delay between relay state toggles in a cycle.</param>
        /// <param name="repeatDelayMs">The delay between repeated relay cycles.</param>
        Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000);

        /// <summary>
        /// Travel in the direction described for a specified amount of time. 
        /// </summary>
        /// <param name="point">A movement direction described by an x,y coordinate that maps to a joystick position on a grid ranged -100,-100 to 100,100 with 0,0 being "stopped"</param>
        /// <param name="duration">The time to move.</param>
        /// <returns>True if the move was successful.</returns>
        Task<bool> Move(Point point, TimeSpan duration);

        /// <summary>
        /// Travel in the direction described by the vector for a specified amount of time.
        /// </summary>
        /// <param name="vector">A vector described by a direction and velocity</param>
        /// <param name="duration">The time to move.</param>
        /// <returns>True if the move was successful.</returns>
        Task<bool> Go(Vector vector, TimeSpan duration);

        /// <summary>
        /// Immediately stop all movements.
        /// </summary>
        void Stop();
    }
}