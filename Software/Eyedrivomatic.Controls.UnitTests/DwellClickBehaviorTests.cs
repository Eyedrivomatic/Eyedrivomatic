using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using FakeItEasy;
using NUnit.Framework;
using Prism.Logging;

using Eyedrivomatic.Controls.UnitTests;

namespace Eyedrivomatic.Controls.DwellClick.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DwellClickBehaviorTests
    {
        private AdornerDecorator _adornerDecorator;
        [Fake] public UIElement AdornedElement { get; set; }
        [Fake] public DwellClickAdorner Adorner { get; set; }
        [Fake] public IDwellClickAnimator Animator { get; set; }

        [UnderTest] public DwellClickBehavior Behavior { get; set; }

        private ILoggerFacade Logger = TestLogging.GetLogger(typeof(DwellClickAnimatorTests));

        [SetUp]
        public void SetUp()
        {
            Fake.InitializeFixture(this);

            DwellClickBehavior.Logger = Logger;

            _adornerDecorator = new AdornerDecorator();
            _adornerDecorator.Child = AdornedElement;
        }

        [Test]
        public void TestMouseEnter()
        {
            AdornedElement.MouseEnter += Raise.With<MouseEventHandler>(AdornedElement, A.Fake<MouseEventArgs>());
            

        }
    }
}
