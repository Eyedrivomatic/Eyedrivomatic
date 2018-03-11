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


using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public abstract class DwellClickAdorner : Adorner
    {
        protected DwellClickAdorner(UIElement adornedElement) : base(adornedElement)
        { }
    
        /// <summary>
        /// A number between 0 and 1 representign the dwell progress.
        /// </summary>
        public double DwellProgress
        {
            get => (double)GetValue(DwellProgressProperty);
            set => SetValue(DwellProgressProperty, value);
        }
        public static readonly DependencyProperty DwellProgressProperty =
            DependencyProperty.Register(nameof(DwellProgress), typeof(double), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(0d, OnDwellProgressChanged));

        private static void OnDwellProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var adorner = d as DwellClickAdorner;
            if (adorner == null) return;
            adorner.ProgressIndicatorVisible = (double)args.NewValue > 0d ? Visibility.Visible : Visibility.Hidden;
            adorner.InvalidateVisual();
        }

        public virtual Visibility ProgressIndicatorVisible
        {
            get => (Visibility)GetValue(ProgressIndicatorVisibleProperty);
            set => SetValue(ProgressIndicatorVisibleProperty, value);
        }
        public static readonly DependencyProperty ProgressIndicatorVisibleProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorVisible), typeof(Visibility), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Visibility.Visible));


        /// <summary>
        /// The size of the progress indicator.
        /// </summary>
        public double ProgressIndicatorSize
        {
            get => (double)GetValue(ProgressIndicatorSizeProperty);
            set => SetValue(ProgressIndicatorSizeProperty, value);
        }
        public static readonly DependencyProperty ProgressIndicatorSizeProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorSize), typeof(double), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(40d));

        public Brush ProgressIndicatorOutline
        {
            get => (Brush)GetValue(ProgressIndicatorOutlineProperty);
            set => SetValue(ProgressIndicatorOutlineProperty, value);
        }
        public static readonly DependencyProperty ProgressIndicatorOutlineProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorOutline), typeof(Brush), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Brushes.Black));


        public Brush ProgressIndicatorFill
        {
            get => (Brush)GetValue(ProgressIndicatorFillProperty);
            set => SetValue(ProgressIndicatorFillProperty, value);
        }
        public static readonly DependencyProperty ProgressIndicatorFillProperty =
            DependencyProperty.Register(nameof(ProgressIndicatorFill), typeof(Brush), typeof(DwellClickAdorner), new FrameworkPropertyMetadata(Brushes.GhostWhite));
    }
}
