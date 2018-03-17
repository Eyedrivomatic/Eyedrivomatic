using System;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using NullGuard;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public static class VisualExtensions
    {
        public static bool IsGazeTarget([AllowNull]this UIElement element)
        {
            return element != null && Interaction.GetBehaviors(element).OfType<DwellClickBehavior>().Any();
        }

        [return:AllowNull]
        public static UIElement GetParentGazeTarget([AllowNull]this UIElement element)
        {
            while (element != null && !element.IsGazeTarget())
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
            return element;
        }

        [return:AllowNull]
        public static HitTestResult GazeHitTest([AllowNull]this UIElement element, Point point, int radius)
        {
            var parameters = radius == 0 
                ? new PointHitTestParameters(point) as HitTestParameters
                : new GeometryHitTestParameters(new EllipseGeometry(point, radius, radius));

            UIElement gazeTarget = null;
            VisualTreeHelper.HitTest(element, null, hitTest =>
            {
                gazeTarget = GetParentGazeTarget(hitTest?.VisualHit as UIElement);
                return gazeTarget != null && !ReferenceEquals(gazeTarget, element) ? HitTestResultBehavior.Stop : HitTestResultBehavior.Continue;
            }, parameters);

            return gazeTarget == null ? null : new PointHitTestResult(gazeTarget, point);
        }
    }
}