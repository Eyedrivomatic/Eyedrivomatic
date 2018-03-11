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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Eyedrivomatic.Controls
{
    public class ImageToggleButton : ToggleButton
    {
        public static readonly DependencyProperty CheckImageProperty =
            DependencyProperty.Register(nameof(CheckImage), typeof(ImageSource), typeof(ImageToggleButton), new PropertyMetadata(null));
        public ImageSource CheckImage
        {
            get => (ImageSource)GetValue(CheckImageProperty);
            set => SetValue(CheckImageProperty, value);
        }
    }
}
