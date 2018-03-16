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
        public static bool IsGazeTarget(this UIElement element)
        {
            return Interaction.GetBehaviors(element).OfType<DwellClickBehavior>().Any();
        }

        [return:AllowNull]
        public static HitTestResult GazeHitTest(this UIElement element, Point point)
        {
            HitTestResult gazeTarget = null;

            VisualTreeHelper.HitTest(element, 
            //    target =>
            //    {
            //        return (target as UIElement)?.IsGazeTarget() ?? false
            //            ? HitTestFilterBehavior.Stop
            //            : HitTestFilterBehavior.ContinueSkipSelf;
            //    },
                null,
                testResult =>
                {
                    if ((testResult.VisualHit as UIElement)?.IsGazeTarget() ?? false)
                    {
                        gazeTarget = testResult;
                        return HitTestResultBehavior.Stop;
                    }
                    return HitTestResultBehavior.Continue;
                },
                new PointHitTestParameters(point));

            if (gazeTarget != null && !ReferenceEquals(element, gazeTarget.VisualHit))
            {
                Console.WriteLine($"Gaze over {gazeTarget?.ToString() ?? "NA"}");
            }
            return gazeTarget;
        }
    }
}