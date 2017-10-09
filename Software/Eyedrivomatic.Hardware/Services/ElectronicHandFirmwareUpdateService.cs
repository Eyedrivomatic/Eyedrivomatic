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

namespace Eyedrivomatic.Hardware.Services
{
    [Export(typeof(IFirmwareUpdateService))]
    public class ElectronicHandFirmwareUpdateService : IFirmwareUpdateService
    {
        public IEnumerable<Version> GetAvailableFirmware(Version minVersion)
        {
            var regex = new Regex(@"Eyedrivomatic.Firmware.(?<Version>(?<Major>[0-9]+)\.(?<Minor>[0-9]+)\.(?<Build>[0-9]+)(\.(?<Revision>[0-9]+))?)");
            var files = GetFirmwareFiles().ToList();

            foreach (var file in files)
            {
                var match = regex.Match(file);
                if (!match.Success) continue;
                var version = new Version(match.Groups["Version"].Value);
                if (version >= minVersion) yield return version;
            }
        }

        public bool HasFirmwareUpdate(IDeviceConnection connection, Version minVersion)
        {
            if (minVersion < connection.FirmwareVersion) minVersion = connection.FirmwareVersion;
            return GetAvailableFirmware(minVersion).Any();
        }

        public Task UpdateLatestFirmwareAsync(IDeviceConnection connection, IProgress<double> progress = null)
        {
            var version = GetAvailableFirmware(connection.FirmwareVersion).OrderByDescending(v => v).FirstOrDefault();
            if (version == null || version == connection.FirmwareVersion) return Task.CompletedTask;

            return UpdateFirmwareAsync(connection, version, progress);
        }

        public async Task UpdateFirmwareAsync(IDeviceConnection connection, Version version, IProgress<double> progress = null)
        {
            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? @".\";
            path = Path.Combine(path, "Firmware", $"Eyedrivomatic.Firmware.{version}.hex");

            connection.Disconnect();

            await Task.Run(() =>
            {
                var uploader = new ArduinoSketchUploader(
                    new ArduinoSketchUploaderOptions
                    {
                        FileName = path,
                        PortName = connection.ConnectionString,
                        ArduinoModel = ArduinoModel.UnoR3,
                    }, null, progress);

                uploader.UploadSketch();
            });

            await connection.ConnectAsync(CancellationToken.None);
        }

        private IEnumerable<string> GetFirmwareFiles()
        {
            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? @".\";
            return Directory.EnumerateFiles(path, "Eyedrivomatic.Firmware.*.hex");
        }
    }
}
