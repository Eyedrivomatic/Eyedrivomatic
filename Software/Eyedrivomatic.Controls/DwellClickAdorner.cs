// Copyright (c) 2016 Eyedrivomatic Authors
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
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using Prism.Logging;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Eyedrivomatic.Controls
{
    public class DwellClickAdorner : Adorner
    {
        private readonly PieSlice _progressIndicator;

        public static ILoggerFacade Logger { get; set; }

        public DwellClickAdorner(UIElement adornedElement) : base(adornedElement)
        {
            Focusable = false;
            IsHitTestVisible = false;

            _progressIndicator = new PieSlice();
            _progressIndicator.HorizontalAlignment = HorizontalAlignment.Center;
            _progressIndicator.VerticalAlignment = VerticalAlignment.Center;
            _progressIndicator.Visibility = Visibility.Visible;

            _progressIndicator.Effect = new DropShadowEffect();

            var binding = new Binding(nameof(DwellProgress));
            binding.Source = this;
            binding.Converter = new ProgressToAngleConverter();
            _progressIndicator.SetBinding(PieSlice.EndAngleProperty, binding);

            binding = new Binding(nameof(ProgressIndicatorVisible));
            binding.Source = this;
            _progressIndicator.SetBinding(VisibilityProperty, binding);

            binding = new Binding(nameof(ProgressIndicatorFill));
            binding.Source = this;
            _progressIndicator.SetBinding(Shape.FillProperty, binding);

            binding = new Binding(nameof(ProgressIndicatorOutline));
            binding.Source = this;
            _progressIndicator.SetBinding(Shape.StrokeProperty, binding);

            binding = new Binding(nameof(ProgressIndicatorRadius));
            binding.Source = this;
            _progressIndicator.SetBinding(WidthProperty, binding);

            binding = new Binding(nameof(ProgressIndicatorRadius));
            binding.Source = this;
            _progressIndicator.SetBinding(HeightProperty, binding);

            AddVisualChild(_progressIndicator);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return _progressIndicator;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _progressIndicator.Measure(constraint);
            return _progressIndicator.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = new Rect(new Point(0, 0), finalSize);
            rect.Offset((AdornedElement.RenderSize.Width - finalSize.Width) / 2d, (AdornedElement.RenderSize.Height - finalSize.Height) / 2d);

            _progressIndicator.Arrange(rect);
            return new Size(_progressIndicator.ActualWidth, _progressIndicator.ActualHeight);
        }


        /// <summary>
        /// A number between 0 and 1 representign the dwell progress.
        /// </summary>
        public double DwellProgress
        {
            get { return (double)GetValue(DwellProgressProperty); }
            set { SetValue(DwellProgressProperty, value); }
        }
        public static readonly DependencyProperty DwellProgressProperty =
            DependencyProperty.Register(nameof(DwellProgress), typeof(double), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(0d, OnDwellProgressChanged));

        private static void OnDwellProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var adorner = d as DwellClickAdorner;
            adorner.SetValue(ProgressIndicatorVisibleProperty, (double)args.NewValue > 0d ? Visibility.Visible : Visibility.Hidden);
            adorner.InvalidateVisual();
        }


        /// <summary>
        /// The size of the progress indicator.
        /// </summary>
        public double ProgressIndicatorRadius
        {
            get { return (double)GetValue(ProgressIndicatorRadiusProperty); }
            set { SetValue(ProgressIndicatorRadiusProperty, value); }
        }
        public static readonly DependencyProperty ProgressIndicatorRadiusProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorRadius), typeof(double), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(40d));


        public Visibility ProgressIndicatorVisible
        {
            get { return (Visibility)GetValue(ProgressIndicatorVisibleProperty); }
            set { SetValue(ProgressIndicatorVisibleProperty, value); }
        }
        public static readonly DependencyProperty ProgressIndicatorVisibleProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorVisible), typeof(Visibility), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Visibility.Visible));


        public Brush ProgressIndicatorOutline
        {
            get { return (Brush)GetValue(ProgressIndicatorOutlineProperty); }
            set { SetValue(ProgressIndicatorOutlineProperty, value); }
        }
        public static readonly DependencyProperty ProgressIndicatorOutlineProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorOutline), typeof(Brush), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Brushes.Black));


        public Brush ProgressIndicatorFill
        {
            get { return (Brush)GetValue(ProgressIndicatorFillProperty); }
            set { SetValue(ProgressIndicatorFillProperty, value); }
        }
        public static readonly DependencyProperty ProgressIndicatorFillProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorFill), typeof(Brush), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Brushes.GhostWhite));


        private class ProgressToAngleConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value % 360.0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((double)value / 360.0) % 1.0;
            }
        }
    }

}
