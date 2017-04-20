using System;
using System.Linq;
using System.Reactive.Linq;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    public abstract class LineMessageProcessor : IBrainBoxMessageProcessor
    {
        private const char Ack = (char)0x06;
        private const char Nak = (char)0x15;
        private bool _disposed;
        protected IDisposable Subscription;

        public void Attach(IObservable<char> source)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);

            Subscription?.Dispose();

            var lineSource = source.Where(c => c != Ack && c != Nak)
                .Publish(chars => chars.Where(c => c != '\n').Buffer(() => chars.Where(c => c == '\n')))
                .Select(ls => new string(ls.ToArray())).Where(s => !string.IsNullOrEmpty(s));

            Subscription = Attach(lineSource);
        }

        protected abstract IDisposable Attach(IObservable<string> source);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Subscription?.Dispose();
            Subscription = null;
        }
    }
}