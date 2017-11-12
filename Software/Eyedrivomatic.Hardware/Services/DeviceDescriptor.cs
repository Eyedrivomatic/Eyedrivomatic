namespace Eyedrivomatic.Hardware.Services
{
    public abstract class DeviceDescriptor
    {
        public string FriendlyName { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{FriendlyName} [{ConnectionString}]";
        }
    }
}