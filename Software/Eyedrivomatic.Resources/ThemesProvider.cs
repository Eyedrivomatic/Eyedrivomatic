﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Markup;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Resources
{
    [UsableDuringInitialization(true)]
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class ThemesProvider
    {
        public IList<ThemeColorsResourceDictionary> Colors { get; } = new List<ThemeColorsResourceDictionary>
        {
            new ThemeColorsResourceDictionary(new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Colors/WindowsDefaultColors.xaml"), "Windows Default")
        };

        public IList<ThemeImagesResourceDictionary> Images { get; } = new List<ThemeImagesResourceDictionary>
        {
            new ThemeImagesResourceDictionary(new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Images/OriginalImages.xaml"), "Original"),
            new ThemeImagesResourceDictionary(new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Images/3DDarkImages.xaml"), "3D Dark"),
            new ThemeImagesResourceDictionary(new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Images/3DLightImages.xaml"), "3D Light"),
        };

        public List<ThemeStylesResourceDictionary> Styles { get; } = new List<ThemeStylesResourceDictionary>
        {
            new ThemeStylesResourceDictionary(new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Themes/Default/DefaultStyles.xaml"), "Default")
        };

    }
}