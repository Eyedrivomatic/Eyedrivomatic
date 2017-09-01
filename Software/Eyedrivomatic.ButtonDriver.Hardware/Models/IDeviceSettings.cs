using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Hardware.Models
{
    public interface IDeviceSettings : INotifyPropertyChanged, IDisposable
    {
        bool IsKnown { get; }
        int CenterPosX { get; set; }
        int MinPosX { get; set; }
        int MaxPosX { get; set; }
        int CenterPosY { get; set; }
        int MinPosY { get; set; }
        int MaxPosY { get; set; }
        bool Switch1Default { get; set; }
        bool Switch2Default { get; set; }
        bool Switch3Default { get; set; }

        Task<bool> Save();
    }
}