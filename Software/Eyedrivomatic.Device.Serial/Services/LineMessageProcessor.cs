//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.Linq;
using System.Reactive.Linq;

namespace Eyedrivomatic.Device.Serial.Services
{
    public abstract class LineMessageProcessor : IDeviceSerialMessageProcessor
    {
        private const char Ack = (char)0x06;
        private const char Nak = (char)0x15;
        private bool _disposed;
        protected IDisposable Subscription;

        public void Attach(IObservable<char> source, IObserver<string> sink)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);

            Subscription?.Dispose();

            var lineSource = source.Where(c => c != Ack && c != Nak)
                .Publish(chars => chars.Where(c => c != '\n').Buffer(() => chars.Where(c => c == '\n')))
                .Select(ls => new string(ls.ToArray())).Where(s => !string.IsNullOrEmpty(s));

            Subscription = Attach(lineSource, sink);
        }

        protected abstract IDisposable Attach(IObservable<string> source, IObserver<string> sink);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Subscription?.Dispose();
            Subscription = null;
        }
    }
}