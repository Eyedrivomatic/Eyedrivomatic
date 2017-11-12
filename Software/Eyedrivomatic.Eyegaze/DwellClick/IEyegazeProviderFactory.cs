namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public interface IEyegazeProviderFactory
    {
        IEyegazeProvider Create(string providerName);
    }
}