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


using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Documents;
using NullGuard;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    [Export]
    public class DwellClickAdornerFactory
    {
        private readonly Func<UIElement, DwellClickAdorner> _exportFactory;

        [ImportingConstructor]
        public DwellClickAdornerFactory(Func<UIElement, DwellClickAdorner> exportFactory)
        {
            _exportFactory = exportFactory;
        }
        
        [return:AllowNull]
        public DwellClickAdorner Create(UIElement adornedElement)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (adornerLayer == null) return null;

            var adorner = _exportFactory(adornedElement);

            if (adorner == null) return null;

            adorner.HorizontalAlignment = HorizontalAlignment.Center;
            adorner.VerticalAlignment = VerticalAlignment.Center;

            adornerLayer.Add(adorner);
            adornerLayer.Update(adornedElement);

            return adorner;
        }
    }
}