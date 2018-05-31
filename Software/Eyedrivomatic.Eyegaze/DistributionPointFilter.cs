using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Eyedrivomatic.Eyegaze
{
    /// <summary>
    /// Based on the work done by Dario D. Salvucci and Joseph H. Goldberg
    /// in their paper Identifying Fixations and Saccades in Eye-Tracking Protocols
    /// http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.68.2459&rep=rep1&type=pdf
    /// </summary>
    public class DistributionPointFilter : IPointFilter
    {
        private readonly TimeSpan _durationThreshold;
        private readonly double _dispertionThreshold;
        private readonly Queue<(Point Pt, DateTime TimeStamp)> _sampleWindow = new Queue<(Point Pt, DateTime TimeStamp)>();

        public DistributionPointFilter(TimeSpan durationThreshold, double dispertionThreshold)
        {
            _durationThreshold = durationThreshold;
            _dispertionThreshold = dispertionThreshold;
        }

        public Point FilterPoint(Point measured)
        {
            var dt = DateTime.Now;


            return measured;
        }
    }
}