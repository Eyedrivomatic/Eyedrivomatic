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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver
{
    [Export("FirmwareUpdateWithConfirmation", typeof(IFirmwareUpdateService))]
    public class FirmwareUpdateConfirmationDecorator : IFirmwareUpdateService
    {
        private readonly InteractionRequest<IFirmwareUpdateProgressNotification> _firmwareUpdateProgress;
        private readonly InteractionRequest<IConfirmationWithCustomButtons> _firmwareUpdateRequest;
        private readonly IFirmwareUpdateService _target;

        [ImportingConstructor]
        public FirmwareUpdateConfirmationDecorator(
            IFirmwareUpdateService target,
            InteractionRequest<IConfirmationWithCustomButtons> firmwareUpdateRequest,
            [Import("FirmwareUpdateProgress")]InteractionRequest<IFirmwareUpdateProgressNotification> firmwareUpdateProgress)
        {
            _target = target;
            _firmwareUpdateRequest = firmwareUpdateRequest;
            _firmwareUpdateProgress = firmwareUpdateProgress;
        }

        public IEnumerable<Version> GetAvailableFirmware()
        {
            return _target.GetAvailableFirmware();
        }

        [return: AllowNull]
        public Version GetLatestVersion()
        {
            return _target.GetLatestVersion();
        }

        public async Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, Version version, bool required, [AllowNull] IProgress<double> progress)
        {
            var requestTask = new TaskCompletionSource<bool>();
            _firmwareUpdateRequest.Raise(
                required
                    ? new ConfirmationWithCustomButtons
                    {
                        Title = Translate.Key(nameof(Strings.Firmware_UpdateRequired_Title)),
                        Content = string.Format(Translate.Key(nameof(Strings.Firmware_UpdateRequired_Directive_Format)),
                            connection.ConnectionString, connection.FirmwareVersion.ToString(3) ?? "N/A",
                            version.ToString(3)),
                        IgnoreDwellPause = true
                    }
                    : new ConfirmationWithCustomButtons
                    {
                        Title = Translate.Key(nameof(Strings.Firmware_UpdateOptional_Title)),
                        Content = string.Format(Translate.Key(nameof(Strings.Firmware_UpdateOptional_Directive_Format)),
                            connection.ConnectionString, connection.FirmwareVersion.ToString(3),
                            version.ToString(3)),
                        IgnoreDwellPause = true
                    },
                c => requestTask.SetResult(c.Confirmed));

            if (!await requestTask.Task) return false;

            var progressTitle = Translate.Key(nameof(Strings.Firmware_Update_Title));
            var progressContent = string.Format(Translate.Key(nameof(Strings.Firmware_Update_Versions_Format)),
                connection.FirmwareVersion.ToString(3),
                version.ToString(3));

            using (var progressNotification = new FirmwareUpdateProgressNotification(connection.FirmwareVersion, version, progress){Title = progressTitle, Content = progressContent})
            {
                _firmwareUpdateProgress.Raise(progressNotification);
                return await _target.UpdateFirmwareAsync(connection, version, required, progressNotification);
            }
        }
    }
}