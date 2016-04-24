﻿// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Eyedrivomatic.Controls
{
    public abstract class DwellProgress
    {


        public void Report(double value)
        {
            Contract.Requires(value >= 0);
            Contract.Requires(value <= 1);
        }
    }


    public class PieSlice : Shape
    {
        // Angle that arc starts at
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        // DependencyProperty - StartAngle
        private static PropertyMetadata startAngleMetadata =
                new PropertyMetadata(
                    0.0,     // Default value
                    null,    // Property changed callback
                    new CoerceValueCallback(CoerceAngle));   // Coerce value callback

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(PieSlice), startAngleMetadata);

        // Angle that arc ends at
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        // DependencyProperty - EndAngle
        private static PropertyMetadata endAngleMetadata =
                new PropertyMetadata(
                    90.0,     // Default value
                    null,    // Property changed callback
                    new CoerceValueCallback(CoerceAngle));   // Coerce value callback

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(PieSlice), endAngleMetadata);

        private static object CoerceAngle(DependencyObject depObj, object baseVal)
        {
            double angle = (double)baseVal;
            angle = Math.Min(angle, 359.9);
            angle = Math.Max(angle, 0.0);
            return angle;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                double maxWidth = Math.Max(0.0, RenderSize.Width - StrokeThickness);
                double maxHeight = Math.Max(0.0, RenderSize.Height - StrokeThickness);
                //Console.WriteLine(string.Format("* maxWidth={0}, maxHeight={1}", maxWidth, maxHeight));

                double xStart = maxWidth / 2.0 * Math.Cos(StartAngle * Math.PI / 180.0);
                double yStart = maxHeight / 2.0 * Math.Sin(StartAngle * Math.PI / 180.0);

                double xEnd = maxWidth / 2.0 * Math.Cos(EndAngle * Math.PI / 180.0);
                double yEnd = maxHeight / 2.0 * Math.Sin(EndAngle * Math.PI / 180.0);

                StreamGeometry geom = new StreamGeometry();
                using (StreamGeometryContext ctx = geom.Open())
                {
                    ctx.BeginFigure(
                        new Point((RenderSize.Width / 2.0) + xStart,
                                  (RenderSize.Height / 2.0) - yStart),
                        true,   // Filled
                        true);  // Closed
                    ctx.ArcTo(
                        new Point((RenderSize.Width / 2.0) + xEnd,
                                  (RenderSize.Height / 2.0) - yEnd),
                        new Size(maxWidth / 2.0, maxHeight / 2),
                        0.0,     // rotationAngle
                        (EndAngle - StartAngle) > 180,   // greater than 180 deg?
                        SweepDirection.Counterclockwise,
                        true,    // isStroked
                        false);
                    ctx.LineTo(new Point((RenderSize.Width / 2.0), (RenderSize.Height / 2.0)), true, false);
                }

                return geom;
            }
        }
    }
}
