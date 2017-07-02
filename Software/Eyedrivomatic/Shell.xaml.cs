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
using System.Windows.Input;

namespace Eyedrivomatic
{
    [Export]
    public partial class Shell : IDisposable
    {
        private readonly Cursor _smallCursor;

        public Shell()
        {
            InitializeComponent();
            var streamResourceInfo = Application.GetResourceStream(
                new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Images/SmallCursor.cur"));

            if (streamResourceInfo != null)
            {
                _smallCursor = new Cursor(streamResourceInfo.Stream);
                Mouse.OverrideCursor = _smallCursor;
            }

            DriveProfileSelection.Items.Clear();
            MainContent.Content = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                Mouse.OverrideCursor = Mouse.OverrideCursor == null ? _smallCursor : null;
                e.Handled = true;
            }
        }

        #region IDisposable Support
        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposeManaged)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposeManaged)
            {
                _smallCursor.Dispose();
            }
        }

        #endregion
    }
}
