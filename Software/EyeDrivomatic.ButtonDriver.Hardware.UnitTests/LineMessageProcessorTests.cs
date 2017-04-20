// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NUnit.Framework;
using Eyedrivomatic.ButtonDriver.Hardware.Services;

namespace EyeDrivomatic.ButtonDriver.Hardware.UnitTests
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

            protected override IDisposable Attach(IObservable<string> source)
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

            sut.Attach("Line1\nLine2\nLine3\n".ToObservable());

            Assert.That(results, Is.EquivalentTo(expectedValues));

        }
    }
}
