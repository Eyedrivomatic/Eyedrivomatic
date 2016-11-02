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
            get { return (ImageSource)GetValue(CheckImageProperty); }
            set { SetValue(CheckImageProperty, value); }
        }
    }
}
