using System;
using System.ComponentModel.Composition;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver
{
    public interface IRequestFirmwareUpdate : IConfirmation
    {
        Version FromVersion { get; }
        Version ToVersion { get; }
        bool Required { get; }
    }

    [Export(typeof(IRequestFirmwareUpdate))]
    public class RequestFirmwareUpdate : IRequestFirmwareUpdate
    {
        public RequestFirmwareUpdate(Version fromVersion, Version toVersion, bool required)
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
            Required = required;
        }

        public string Title { get; set; }
        public object Content { get; set; }
        public bool Confirmed { get; set; }
        public Version FromVersion { get; }
        public Version ToVersion { get; }
        public bool Required { get; }
    }
}
