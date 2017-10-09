namespace Eyedrivomatic.Hardware.Communications
{
    public interface IElectronicHandConnectionFactory
    {
        IDeviceConnection CreateConnection(string connectionString);
    }
}