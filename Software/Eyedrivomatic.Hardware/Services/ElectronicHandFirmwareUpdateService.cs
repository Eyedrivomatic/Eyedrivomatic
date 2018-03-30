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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ArduinoUploader;
using ArduinoUploader.Hardware;
using Eyedrivomatic.Hardware.Communications;
using NullGuard;

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IFirmwareUpdateService))]
    public class ElectronicHandFirmwareUpdateService : IFirmwareUpdateService
    {
        private readonly IArduinoUploaderLogger _uploaderLogger;

        [ImportingConstructor]
        public ElectronicHandFirmwareUpdateService(IArduinoUploaderLogger uploaderLogger)
        {
            _uploaderLogger = uploaderLogger;
        }

        public IEnumerable<Version> GetAvailableFirmware()
        {
            var regex = new Regex(@"Eyedrivomatic.Firmware.(?<Version>(?<Major>[0-9]+)\.(?<Minor>[0-9]+)\.(?<Build>[0-9]+)(\.(?<Revision>[0-9]+))?).hex");
            var files = GetFirmwareFiles().ToList();

            foreach (var file in files)
            {
                var match = regex.Match(file);
                if (!match.Success) continue;
                yield return new Version(match.Groups["Version"].Value);
            }
        }

        [return:AllowNull]
        public Version GetLatestVersion()
        {   
            return GetAvailableFirmware().OrderByDescending(v => v).FirstOrDefault();
        }

        public async Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, Version version, bool required, IProgress<double> progress = null)
        {
            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? @".\";
            path = Path.Combine(path, "Firmware", $"Eyedrivomatic.Firmware.{version ?? GetLatestVersion()}.hex");

            connection.Disconnect();
            await Task.Delay(TimeSpan.FromSeconds(2));

            await Task.Run(() =>
            {
                var uploader = new ArduinoSketchUploader(
                    new ArduinoSketchUploaderOptions
                    {
                        FileName = path,
                        PortName = connection.ConnectionString,
                        ArduinoModel = ArduinoModel.UnoR3
                    }, _uploaderLogger, progress);

                uploader.UploadSketch();
            });

            await connection.ConnectAsync(CancellationToken.None);
            return connection.State == ConnectionState.Connected;
        }

        private IEnumerable<string> GetFirmwareFiles()
        {
            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? @".\";
            path = Path.Combine(path, "Firmware");
            return Directory.Exists(path) 
                ? Directory.EnumerateFiles(path, "Eyedrivomatic.Firmware.*.hex") 
                : Enumerable.Empty<string>();
        }
    }
}
