using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    public class AncestorOpacityMask
    {
        public static readonly DependencyProperty MaskingElementsProperty = DependencyProperty.RegisterAttached(
            "MaskingElements", typeof(IList<FrameworkElement>), typeof(AncestorOpacityMask), new PropertyMetadata(default(IList<FrameworkElement>)));

        private static readonly DependencyProperty BaseMaskProperty = DependencyProperty.RegisterAttached(
            "BaseMask", typeof(Brush), typeof(AncestorOpacityMask), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty MaskableProperty = DependencyProperty.RegisterAttached(
            "Maskable", typeof(FrameworkElement), typeof(AncestorOpacityMask), new PropertyMetadata(default(FrameworkElement), MaskableChangedCallback));

        private static void MaskableChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var maskingElement = dependencyObject as FrameworkElement;
            if (maskingElement == null) return;

            if (args.OldValue is FrameworkElement oldMaskable)
            {
                var oldMaskingElements = (IList<FrameworkElement>)oldMaskable.GetValue(MaskingElementsProperty);
                oldMaskingElements?.Remove(maskingElement);
                ApplyMask(oldMaskable);
            }

            var newMaskable = args.NewValue as FrameworkElement;
            if (newMaskable == null) return;

            newMaskable.SetValue(BaseMaskProperty, newMaskable.OpacityMask);

            if (GetEnabled(dependencyObject))
            {
                var newMaskingElements = GetMaskingElements(newMaskable);
                newMaskingElements.Add(maskingElement);
            }

            ApplyMask(newMaskable);
        }

        private static IList<FrameworkElement> GetMaskingElements(FrameworkElement maskable)
        {
            var newMaskingElements = (IList<FrameworkElement>) maskable.GetValue(MaskingElementsProperty);
            if (newMaskingElements == null)
            {
                newMaskingElements = new List<FrameworkElement>();
                maskable.SetValue(MaskingElementsProperty, newMaskingElements);
            }
            return newMaskingElements;
        }

        public static void SetMaskable(DependencyObject element, [AllowNull] FrameworkElement value)
        {
            element.SetValue(MaskableProperty, value);
        }
        
        [return: AllowNull]
        public static FrameworkElement GetMaskable(DependencyObject element)
        {
            return (FrameworkElement) element?.GetValue(MaskableProperty);
        }

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(AncestorOpacityMask), new PropertyMetadata(default(bool), EnabledChangedCallback));

        private static void EnabledChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if ((bool) args.NewValue == (bool) args.OldValue) return;

            var element = dependencyObject as FrameworkElement;
            if (element == null) return;
            var enabled = (bool) args.NewValue;

            var maskable = GetMaskable(dependencyObject);
            if (maskable == null) return;

            var newMaskingElements = GetMaskingElements(maskable);

            if (enabled)
            {
                newMaskingElements.Add(element);
                element.SizeChanged += ElementOnSizeChanged;
                maskable.SizeChanged += MaskableOnSizeChanged;
            }
            else
            {
                newMaskingElements.Remove(element);
                element.SizeChanged -= ElementOnSizeChanged;
                maskable.SizeChanged -= MaskableOnSizeChanged;
            }

            ApplyMask(maskable);
        }

        private static void ElementOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var element = (FrameworkElement)sender;
            var maskable = GetMaskable(element);
            if (maskable == null) return;

            ApplyMask(maskable);
        }

        private static void MaskableOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var maskable = sender as FrameworkElement;
            if (maskable == null) return;
            ApplyMask(maskable);
        }

        public static void SetEnabled(DependencyObject element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        public static bool GetEnabled(DependencyObject element)
        {
            return (bool) element.GetValue(EnabledProperty);
        }

        private static void ApplyMask(FrameworkElement maskableElement)
        {
            var maskingElements = GetMaskingElements(maskableElement);
            var baseMask = maskableElement.GetValue(BaseMaskProperty) as Brush;

            if (maskingElements == null || !maskingElements.Any()) 
            {
                maskableElement.OpacityMask = baseMask;
                return;
            }

            var mask = CreateMaskFromElements(maskingElements, maskableElement, baseMask);
            if (mask == null) return;

            var opacityMask = new VisualBrush(mask);
            maskableElement.OpacityMask = opacityMask;
        }

        private static Visual CreateMaskFromElements(IEnumerable<FrameworkElement> maskingElements, FrameworkElement maskableElement, Brush baseMask)
        {
            var maskableBounds =  new Rect(new Size(maskableElement.ActualWidth, maskableElement.ActualHeight));

            var maskVisual = new DrawingVisual();
            Geometry maskRegions = new RectangleGeometry(maskableBounds);

            using (var ctx = maskVisual.RenderOpen())
            {
                foreach (var maskingElement in maskingElements)
                {
                    var visualTransform = maskingElement.TransformToVisual(maskableElement);
                    var childBounds = new Rect(visualTransform.Transform(new Point(0,0)), maskingElement.RenderSize);

                    if (Math.Abs(childBounds.Width) < 1f || Math.Abs(childBounds.Height) < 1f) continue;

                    ctx.DrawRectangle(new VisualBrush(maskingElement), null, childBounds);
                    maskRegions = new CombinedGeometry(GeometryCombineMode.Exclude, maskRegions, new RectangleGeometry(childBounds));
                }

                //ctx.DrawRectangle(Brushes.Transparent, null, maskableBounds);
                ctx.PushClip(maskRegions);
                ctx.DrawRectangle(baseMask ?? Brushes.Black, null, maskableBounds);
                ctx.Pop();
            }

            return maskVisual;
        }
    }
}
