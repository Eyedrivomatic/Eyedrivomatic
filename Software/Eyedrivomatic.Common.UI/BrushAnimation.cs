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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NullGuard;

namespace Eyedrivomatic.Common.UI
{

    //Thank you to Koopakiller
    //https://stackoverflow.com/a/29659723/2529742
    public class BrushAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(Brush);

        [return: AllowNull]
        public override object GetCurrentValue([AllowNull] object defaultOriginValue,
            [AllowNull] object defaultDestinationValue,
            AnimationClock animationClock)
        {
            return GetCurrentValue(defaultOriginValue as Brush,
                defaultDestinationValue as Brush,
                animationClock);
        }

        [return: AllowNull]
        public object GetCurrentValue([AllowNull] Brush defaultOriginValue,
            [AllowNull] Brush defaultDestinationValue,
            AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
                return Brushes.Transparent;

            //use the standard values if From and To are not set 
            //(it is the value of the given property)
            defaultOriginValue = From ?? defaultOriginValue;
            defaultDestinationValue = To ?? defaultDestinationValue;

            if (Math.Abs(animationClock.CurrentProgress.Value) < 0.0001)
                return defaultOriginValue;
            if (Math.Abs(animationClock.CurrentProgress.Value - 1) < 0.0001)
                return defaultDestinationValue;

            return new VisualBrush(new Border()
            {
                Width = 1,
                Height = 1,
                Background = defaultOriginValue,
                Child = new Border()
                {
                    Background = defaultDestinationValue,
                    Opacity = animationClock.CurrentProgress.Value,
                }
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }

        //we must define From and To, AnimationTimeline does not have this properties
        [AllowNull]
        public Brush From
        {
            get => (Brush) GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        [AllowNull]
        public Brush To
        {
            get => (Brush) GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));
    }
}