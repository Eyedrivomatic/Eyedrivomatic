using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Eyedrivomatic.Controls.DwellClick;
using FakeItEasy;
using NUnit.Framework;

namespace Eyedrivomatic.Controls.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DwellClickBehaviorTests
    {
        private AdornerDecorator _adornerDecorator;
        [Fake] public UIElement AdornedElement { get; set; }
        [Fake] public DwellClickAdorner Adorner { get; set; }
        [Fake] public IDwellClickAnimator Animator { get; set; }

        [UnderTest] public DwellClickBehavior Behavior { get; set; }

        [SetUp]
        public void SetUp()
        {
            Fake.InitializeFixture(this);

            _adornerDecorator = new AdornerDecorator {Child = AdornedElement};
        }

        [Test]
        public void TestMouseEnter()
        {
            AdornedElement.MouseEnter += Raise.With<MouseEventHandler>(AdornedElement, A.Fake<MouseEventArgs>());
            

        }
    }
}
