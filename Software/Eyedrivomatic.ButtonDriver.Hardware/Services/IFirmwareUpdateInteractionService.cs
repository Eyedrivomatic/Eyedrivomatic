using System;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    interface IFirmwareUpdateInteractionService
    {
        bool RequestFirmwareUpdate(string connectionString, bool required, Version currentVersion, Version newVersion);
        void NotifyFirmwareUpdateFailed(string message);
    }
}
