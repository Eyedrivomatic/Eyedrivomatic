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
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Eyedrivomatic.Device.Serial.Services;
using NUnit.Framework;

namespace Eyedrivomatic.Device.Serial.UnitTests
{
    [TestFixture]
    public class LineMessageProcessorTests
    {
        public class LineMessageProcessorTester : LineMessageProcessor
        {
            private readonly Func<IObservable<string>, IDisposable> _doOnAttach;

            public LineMessageProcessorTester(Func<IObservable<string>, IDisposable> doOnAttach)
            {
                _doOnAttach = doOnAttach;
            }

            protected override IDisposable Attach(IObservable<string> source, IObserver<string> sink)
            {
                return _doOnAttach(source);
            }
        }

        [Test]
        public void TestLineMessageProcessor_Works()
        {
            var expectedValues = new [] {"Line1", "Line2", "Line3"};
            var results = new List<string>();
            var sut = new LineMessageProcessorTester(source =>
            {
                source.Subscribe(results.Add);
                return null;
            });

            sut.Attach("Line1\nLine2\nLine3\n".ToObservable(), new AnonymousObserver<string>(message => Console.WriteLine($"MESSAGE => {message}")));

            Assert.That(results, Is.EquivalentTo(expectedValues));

        }
    }
}
