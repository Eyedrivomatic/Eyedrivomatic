using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Models
{
    [ContractClass(typeof(Contracts.DeviceSettingsContract))]
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
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IDeviceSettings))]
        internal abstract class DeviceSettingsContract : IDeviceSettings
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public abstract bool IsKnown { get; }
            public abstract int CenterPosX { get; set; }
            public abstract int MinPosX { get; set; }
            public abstract int MaxPosX { get; set; }
            public abstract int CenterPosY { get; set; }
            public abstract int MinPosY { get; set; }
            public abstract int MaxPosY { get; set; }
            public abstract bool Switch1Default { get; set; }
            public abstract bool Switch2Default { get; set; }
            public abstract bool Switch3Default { get; set; }

            public abstract void Dispose();

            [ContractInvariantMethod]
            private void ContractInvariant()
            {
                Contract.Invariant(MaxPosX > CenterPosX, "X-servo max position must be greater than its center position.");
                Contract.Invariant(MinPosX < CenterPosX, "X-servo min position must be less than its center position.");
                Contract.Invariant(MaxPosY > CenterPosY, "Y-servo max position must be greater than its center position.");
                Contract.Invariant(MinPosY < CenterPosY, "Y-servo min position must be less than its center position.");
            }

        }
    }
}