using System;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    public interface IBrainBoxMessageProcessor : IDisposable
    {
        void Attach(IObservable<char> source);
    }
}
