using System;

namespace Eyedrivomatic.Device.Communications
{
    public class VersionInfo
    {
        public static VersionInfo Unknown = new VersionInfo("N/A", new Version(0,0,0));

        public VersionInfo(string model, Version version, string variant = null)
        {
            Model = model;
            Variant = variant ?? string.Empty;
            Version = version;
        }

        public string Model { get; }
        public string Variant { get; }
        public Version Version { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Variant)
                ? $"{Model} {Version.ToString(3)}"
                : $"{Model} {Version.ToString(3)} ({Variant} build)";
        }
    }
}