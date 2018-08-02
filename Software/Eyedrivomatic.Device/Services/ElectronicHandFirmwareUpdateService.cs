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
using Eyedrivomatic.Device.Communications;
using NullGuard;

namespace Eyedrivomatic.Device.Services
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

        public IEnumerable<VersionInfo> GetAvailableFirmware()
        {
            var regex = new Regex(@"Eyedrivomatic\.Firmware\.((?<Model>\w)\.)((?<Variant>\w+)\.)?(?<Version>(?<Major>[0-9]+)\.(?<Minor>[0-9]+)\.(?<Build>[0-9]+)(\.(?<Revision>[0-9]+))?).hex");
            var files = GetFirmwareFiles().ToList();

            foreach (var file in files)
            {
                var match = regex.Match(file);
                if (!match.Success) continue;
                var model = match.Groups["Model"].Value;
                var version = new Version(match.Groups["Version"].Value);
                var variant = match.Groups["Variant"].Value;
                yield return new VersionInfo(model, version, variant);
            }
        }

        [return:AllowNull]
        public VersionInfo GetLatestVersion(string model, string variant)
        {   
            return GetAvailableFirmware()
                .Where(v => string.IsNullOrEmpty(model) || string.CompareOrdinal(v.Model, model) == 0)
                .Where(v => string.IsNullOrEmpty(variant) || string.CompareOrdinal(v.Variant, variant) == 0)
                .OrderByDescending(v => v).FirstOrDefault();
        }

        public async Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, VersionInfo version, bool required, IProgress<double> progress = null)
        {
            if (version == null) version = GetLatestVersion(connection.VersionInfo.Model, null) ?? throw new InvalidOperationException("Version not supplied.");
            var fileName = string.IsNullOrEmpty(version.Variant)
                ? $"Eyedrivomatic.Firmware.{version.Model}.{version.Version}.hex"
                : $"Eyedrivomatic.Firmware.{version.Model}.{version.Variant}.{version.Version}.hex";

            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? @".\";
            path = Path.Combine(path, "Firmware", fileName);

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
