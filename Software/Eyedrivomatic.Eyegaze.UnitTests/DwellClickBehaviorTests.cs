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


using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Eyedrivomatic.Eyegaze.DwellClick;
using FakeItEasy;
using NUnit.Framework;

namespace Eyedrivomatic.Eyegaze.UnitTests
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
            AdornedElement.MouseEnter += Raise.FreeForm.With(AdornedElement, A.Fake<MouseEventArgs>());   
        }
    }
}
