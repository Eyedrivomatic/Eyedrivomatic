using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Serial;
using Eyedrivomatic.Device.Serial.Services;

namespace Eyedrivomatic.Device.Delta
{
    public class DeltaDeviceFactory
    {
        private readonly Lazy<IDeviceCommands> _commands;
        private readonly IList<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;
        private readonly Lazy<IDeviceStatus> _deviceStatus;
        private readonly Lazy<IDeviceSettings> _deviceSettings;

        [ImportingConstructor]
        public DeltaDeviceFactory(Lazy<IDeviceCommands> commands, 
            [ImportMany] IEnumerable<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> messageProcessors, 
            [Import("Delta")] Lazy<IDeviceStatus> deviceStatus, 
            [Import("Delta")] Lazy<IDeviceSettings> deviceSettings)
        {
            _commands = commands;
            _messageProcessors = messageProcessors.ToList();
            _deviceStatus = deviceStatus;
            _deviceSettings = deviceSettings;
        }

        [Export(typeof(Func<IDeviceConnection, IDevice>)),
         ExportMetadata("DeviceType", "Delta")]
        IDevice CreateDevice(IDeviceConnection connection)
        {
            return  new DeltaDevice(connection, _commands.Value, _messageProcessors, _deviceStatus.Value, _deviceSettings.Value);
        }
    }

    public class DeltaDevice : BaseSerialDevice
    {
        public DeltaDevice(IDeviceConnection connection, IDeviceCommands commandFactory, 
            IEnumerable<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> messageProcessors, 
            IDeviceStatus deviceStatus, 
            IDeviceSettings deviceSettings) 
            : base(connection, commandFactory, messageProcessors, deviceStatus, deviceSettings)
        {}
    }
}
