using System;
using System.Threading;
using System.Windows;

using FakeItEasy;
using NUnit.Framework;
using System.Threading.Tasks;
using Prism.Logging;

using Eyedrivomatic.Controls.UnitTests;

namespace Eyedrivomatic.Controls.DwellClick.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DwellClickAnimatorTests
    {
        [Fake] public UIElement AdornedElement { get; set; }
        [Fake] public DwellClickAdorner Adorner { get; set; }

        [UnderTest] public DwellClickAnimator Animator { get; set; }

        private ILoggerFacade Logger = TestLogging.GetLogger(typeof(DwellClickAnimatorTests));

        [SetUp]
        public void SetUp()
        {
            Fake.InitializeFixture(this);
            DwellClickAnimator.Logger = TestLogging.GetLogger(typeof(DwellClickAnimator));
            DwellClickAdorner.Logger = TestLogging.GetLogger(typeof(DwellClickAdorner));
        }

        [Test, Timeout(200)]
        public async Task TestRunAnimation()
        {
            var tcs = new TaskCompletionSource<bool>();
            Action callback = () => tcs.TrySetResult(true);

            Animator.StartAnimation(Adorner, TimeSpan.FromMilliseconds(100), callback);

            //TODO: Figure out why the storyboard doesn't run.
            // I think this has to do with the Dispatcher interaction as the storyboard is normally created on the UI thread.
            // Whereas in NUnit it is created on a threadpool thread. 
            await tcs.Task;

            Assert.That(tcs.Task.Status == TaskStatus.RanToCompletion);

            Assert.That(Adorner.DwellProgress, Is.EqualTo(100));
            A.CallToSet<Visibility>(() => Adorner.ProgressIndicatorVisible).To(Visibility.Visible).MustHaveHappened();
            A.CallToSet<Visibility>(() => Adorner.ProgressIndicatorVisible).To(Visibility.Hidden).MustHaveHappened();
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
