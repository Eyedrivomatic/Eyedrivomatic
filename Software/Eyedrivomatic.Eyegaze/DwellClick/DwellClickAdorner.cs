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


using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public static class DwellClickAdornerFactory
    {
        [Import]
        public static Func<UIElement, DwellClickAdorner> Create;
    }

    public abstract class DwellClickAdorner : Adorner
    {
        internal static DwellClickAdorner CreateAndAdd(UIElement adornedElement)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (adornerLayer == null) return null;

            var adorner = DwellClickAdornerFactory.Create?.Invoke(adornedElement);

            if (adorner == null) return null;
           
            adorner.HorizontalAlignment = HorizontalAlignment.Center;
            adorner.VerticalAlignment = VerticalAlignment.Center;

            adornerLayer.Add(adorner);
            adornerLayer.Update(adornedElement);

            return adorner;
        }

        public DwellClickAdorner(UIElement adornedElement) : base(adornedElement)
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
            if (d == null) return;
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
