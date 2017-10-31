using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Hardware.Services;
using Eyedrivomatic.Resources;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver
{
    [Export("FirmwareUpdateWithConfirmation", typeof(IFirmwareUpdateService))]
    public class FirmwareUpdateConfirmationDecorator : IFirmwareUpdateService
    {
        private readonly IFirmwareUpdateService _target;
        private readonly InteractionRequest<IConfirmation> _firmwareUpdateRequest;
        [ImportingConstructor]
        public FirmwareUpdateConfirmationDecorator(
            IFirmwareUpdateService target, 
            InteractionRequest<IConfirmation> firmwareUpdateRequest)
        {
            _target = target;
            _firmwareUpdateRequest = firmwareUpdateRequest;
        }

        public IEnumerable<Version> GetAvailableFirmware()
        {
            return _target.GetAvailableFirmware();
        }

        public Version GetLatestVersion()
        {
            return _target.GetLatestVersion();
        }

        public async Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, Version version, bool required, IProgress<double> progress = null)
        {
            TaskCompletionSource<bool> requestTask = new TaskCompletionSource<bool>();
            _firmwareUpdateRequest.Raise(
                required
                    ? new Confirmation
                    {
                        Title = Strings.Firmware_UpdateRequired_Title,
                        Content = string.Format(Strings.Firmware_UpdateRequired_Directive_Format,
                            connection.ConnectionString, connection.FirmwareVersion.ToString(3) ?? "N/A",
                            version.ToString(3))
                    }
                    : new Confirmation
                    {
                        Title = Strings.Firmware_UpdateOptional_Title,
                        Content = string.Format(Strings.Firmware_UpdateOptional_Directive_Format,
                            connection.ConnectionString, connection.FirmwareVersion.ToString(3) ?? "N/A",
                            version.ToString(3))
                    },
                c => requestTask.SetResult(c.Confirmed));

            if (!await requestTask.Task) return false;

            return await _target.UpdateFirmwareAsync(connection, version, required, progress);
        }
    }
}
