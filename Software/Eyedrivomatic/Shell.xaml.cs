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
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;

using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic
{
    [Export]
    public partial class Shell : Window, IPartImportsSatisfiedNotification
    {
        public Shell()
        {
            InitializeComponent();
            //Mouse.OverrideCursor = ((FrameworkElement)Resources["SmallCursor"]).Cursor;
        }

        public void OnImportsSatisfied()
        {
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (Mouse.OverrideCursor == null) Mouse.OverrideCursor = ((FrameworkElement)Resources["SmallCursor"]).Cursor;
                else Mouse.OverrideCursor = null;
                e.Handled = true;
            }
        }
    }
}