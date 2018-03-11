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
using System.Windows;
using System.Windows.Media.Animation;

namespace Eyedrivomatic.Infrastructure
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(GridLength), typeof(GridLengthAnimation));
        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(nameof(To), typeof(GridLength), typeof(GridLengthAnimation));
        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue,
            object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null) return defaultOriginValue;


            var from = (GridLength)GetValue(FromProperty);
            if (from.IsAuto) from = (GridLength) defaultOriginValue;

            var to = (GridLength)GetValue(ToProperty);
            if (to.IsAuto) to = (GridLength) defaultDestinationValue;

            if (from.GridUnitType != to.GridUnitType) throw new InvalidOperationException("Both From and To unit types must match.");
            if (from.IsAuto) return from;

            var newVal = from.Value > to.Value
                ? (1 - animationClock.CurrentProgress.Value) * (from.Value - to.Value) + to.Value
                : animationClock.CurrentProgress.Value * (to.Value - from.Value) + from.Value;

            return new GridLength(newVal, from.GridUnitType);
        }
    }
}
