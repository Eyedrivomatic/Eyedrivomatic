using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;

namespace Eyedrivomatic.Device.Services
{
    [Export(typeof(IDeviceFactory))]
    internal class DeviceFactory : IDeviceFactory
    {
        private readonly IConnectionEnumerationService _connectionEnumerationService;
        private readonly IDeviceConnectionFactory _connectionFactory;
        private readonly IFirmwareUpdateService _firmwareUpdateService;
        private readonly string _variant;
        private readonly IList<ExportFactory<Func<IDeviceConnection, IDevice>, IDeviceFactoryExportMetadata>> _deviceFactories;

        public static Version MinFirmwareVersion = new Version(2, 1, 0);

        [ImportingConstructor]
        public DeviceFactory(
            IConnectionEnumerationService connectionEnumerationService,
            IDeviceConnectionFactory connectionFactory,
            [Import("FirmwareUpdateWithConfirmation")] IFirmwareUpdateService firmwareUpdateService,
            [Import("DeviceVariant")] string variant,
            [ImportMany]IEnumerable<ExportFactory<Func<IDeviceConnection, IDevice>, IDeviceFactoryExportMetadata>> deviceFactories)
        {
            _connectionEnumerationService = connectionEnumerationService;
            _connectionFactory = connectionFactory;
            _firmwareUpdateService = firmwareUpdateService;
            _variant = variant;
            _deviceFactories = deviceFactories.ToList();
        }

        public IList<DeviceDescriptor> GetAvailableDevices()
        {
            return _connectionEnumerationService.GetAvailableDevices(true);
        }

        [return:AllowNull]
        public async Task<IDevice> AutoConnectAsync(bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            var connection = await _connectionEnumerationService.DetectDeviceAsync(MinFirmwareVersion, cancellationToken);
            if (connection == null)
            {
                Log.Error(this, "Device not found!");
                throw new ConnectionFailedException(Strings.DeviceConnection_Error_Auto_NotFound);
            }

            await CheckFirmwareVersion(connection, autoUpdateFirmware);

            var factory = _deviceFactories.SingleOrDefault(d => StringComparer.OrdinalIgnoreCase.Compare(d.Metadata.DeviceType, connection.VersionInfo.Model) == 0);
            return factory?.CreateExport().Value(connection);
        }

        [return:AllowNull]
        public async Task<IDevice> ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            var device = _connectionEnumerationService.GetAvailableDevices(true).FirstOrDefault(
                d => StringComparer.OrdinalIgnoreCase.Compare(d.ConnectionString, connectionString) == 0);

            if (device == null)
            {
                Log.Error(this, $"Device [{connectionString}] not found!");
                throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_Manual_NotFound, connectionString));
            }

            var connection = _connectionFactory.CreateConnection(device);
            await connection.ConnectAsync(cancellationToken);

            if (connection.State != ConnectionState.Connected)
            {
                if (_connectionEnumerationService.GetAvailableDevices(false)
                    .All(d => StringComparer.OrdinalIgnoreCase.Compare(d.ConnectionString, connectionString) != 0))
                {
                    Log.Error(this, $"Connection to device [{connectionString}] failed!");

                    throw new ConnectionFailedException(
                        String.Format(Strings.DeviceConnection_Error_Manual, connectionString));
                }
            }

            await CheckFirmwareVersion(connection, autoUpdateFirmware);
            var factory = _deviceFactories.SingleOrDefault(d => StringComparer.OrdinalIgnoreCase.Compare(d.Metadata.DeviceType, connection.VersionInfo.Model) == 0);
            return factory?.CreateExport().Value(connection);
        }

        private async Task CheckFirmwareVersion(IDeviceConnection connection, bool autoUpdateFirmware)
        {
            var latestVersion = _firmwareUpdateService.GetLatestVersion(connection.VersionInfo.Model, _variant);

            //Required update.
            if (!String.IsNullOrEmpty(_variant) && StringComparer.OrdinalIgnoreCase.Compare(connection.VersionInfo.Variant, _variant) != 0)
            {
                if (latestVersion == null || latestVersion.Version < MinFirmwareVersion)
                {
                    Log.Error(this, $"A device was detected with firmware for [{(String.IsNullOrEmpty(connection.VersionInfo.Variant) ? "standard" : connection.VersionInfo.Variant)}] hardware, however firmware for [{_variant}] is required. Unfortunately the firmware file cannot be found.");
                    throw new ConnectionFailedException(Strings.DeviceConnection_MinFirmwareNotAvailable);
                }

                if (!autoUpdateFirmware || !await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, true))
                    throw new ConnectionFailedException(String.Format(Strings.DeviceConnection_Error_FirmwareCheck, connection.ConnectionString));
            }

            if (connection.VersionInfo.Version < MinFirmwareVersion)
            {
                if (latestVersion == null || latestVersion.Version < MinFirmwareVersion)
                {
                    Log.Error(this, $"A device was detected with firmware version [{connection.VersionInfo.Version}], However a minimum version [{MinFirmwareVersion}] is required. However the firmware file cannot be found.");
                    throw new ConnectionFailedException(Strings.DeviceConnection_MinFirmwareNotAvailable);
                }

                if (!autoUpdateFirmware || !await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, true))
                    throw new ConnectionFailedException(String.Format(Strings.DeviceConnection_Error_FirmwareCheck, connection.ConnectionString));
            }

            if (autoUpdateFirmware && latestVersion != null && latestVersion.Version > connection.VersionInfo.Version)
            {
                await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, false);
            }
        }
    }
}