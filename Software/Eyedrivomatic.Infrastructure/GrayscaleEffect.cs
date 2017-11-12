using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace Eyedrivomatic.Infrastructure
{
    /// <summary>
    /// Applies the grayscale pixel shader to an element.
    /// Thank you to Anders Bursjöö
    /// http://bursjootech.blogspot.com/2008/06/grayscale-effect-pixel-shader-effect-in.html
    /// </summary>
    public class GrayscaleEffect : ShaderEffect
    {
        public static readonly PixelShader GrayscalePixelShader = new PixelShader() { UriSource = new Uri(@"pack://application:,,,/Eyedrivomatic.Infrastructure;component/GrayscaleEffect.ps") };

        public GrayscaleEffect()
        {
            PixelShader = GrayscalePixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DesaturationFactorProperty);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty DesaturationFactorProperty = DependencyProperty.Register("DesaturationFactor", typeof(double), typeof(GrayscaleEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0), CoerceDesaturationFactor));
        public double DesaturationFactor
        {
            get => (double)GetValue(DesaturationFactorProperty);
            set => SetValue(DesaturationFactorProperty, value);
        }

        private static object CoerceDesaturationFactor(DependencyObject d, object value)
        {
            GrayscaleEffect effect = (GrayscaleEffect)d;
            double newFactor = (double)value;

            if (newFactor < 0.0 || newFactor > 1.0)
            {
                return effect.DesaturationFactor;
            }

            return newFactor;
        }
    }
}