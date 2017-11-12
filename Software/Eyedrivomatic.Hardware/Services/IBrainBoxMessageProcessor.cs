using System;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IBrainBoxMessageProcessor : IDisposable
    {
        void Attach(IObservable<char> source, IObserver<string> sender);
    }
}
