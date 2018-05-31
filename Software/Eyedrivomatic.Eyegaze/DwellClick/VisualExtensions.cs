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
        public static UIElement GetVisualRoot([AllowNull]this UIElement element)
        {
            if (element == null) return null;

            var parent = VisualTreeHelper.GetParent(element) as UIElement;
            return parent == null ? element : GetVisualRoot(parent);
        }

        [return:AllowNull]
        public static HitTestResult GazeHitTest([AllowNull]this UIElement element, Point point)
        {
            UIElement gazeTarget = null;
            VisualTreeHelper.HitTest(element, null, hitTest =>
            {
                gazeTarget = GetParentGazeTarget(hitTest?.VisualHit as UIElement);
                return gazeTarget != null && !ReferenceEquals(gazeTarget, element) ? HitTestResultBehavior.Stop : HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(point));

            return gazeTarget == null ? null : new PointHitTestResult(gazeTarget, point);
        }
    }
}