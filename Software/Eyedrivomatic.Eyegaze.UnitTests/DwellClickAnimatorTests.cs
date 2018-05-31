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
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using System.Windows;
using System.Windows.Threading;
using Eyedrivomatic.Eyegaze.DwellClick;

namespace Eyedrivomatic.Eyegaze.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DwellClickAnimatorTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        //[Timeout(200)]
        public async Task TestRunAnimation()
        {
            var adorner = A.Fake<DwellClickAdorner>();

            object Test(object o)
            {
                var animator = new DwellClickAnimator();
                animator.StartAnimation(adorner, TimeSpan.FromMilliseconds(100), () => Dispatcher.ExitAllFrames());

                return null;
            }

            var dispatcherTask = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(Test),
                null);

            Dispatcher.Run();

            await dispatcherTask;

            Assert.That(adorner.DwellProgress, Is.EqualTo(100));
            A.CallToSet(() => adorner.ProgressIndicatorVisible).To(Visibility.Visible).MustHaveHappened();
            A.CallToSet(() => adorner.ProgressIndicatorVisible).To(Visibility.Hidden).MustHaveHappened();
        }

        [Test]
        public void TestPauseAnimation()
        {
            Assert.Fail("Test not Implemented");
        }

        [Test]
        public void TestResumeAnimation()
        {
            Assert.Fail("Test not Implemented");
        }

        [Test]
        public void TestStopAnimation()
        {
            Assert.Fail("Test not Implemented");
        }
    }
}
