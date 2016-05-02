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
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using Prism.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Eyedrivomatic.Controls
{
    public class BrowsableList : ListBox
    {
        static BrowsableList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowsableList), new FrameworkPropertyMetadata(typeof(BrowsableList)));
        }

        public BrowsableList()
        {
            _prevItemCommand = new DelegateCommand(() => SelectedIndex--, () => SelectedIndex > 0);
            _nextItemCommand = new DelegateCommand(() => SelectedIndex++, () => SelectedIndex < Items.Count - 1);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            _prevItemCommand.RaiseCanExecuteChanged();
            _nextItemCommand.RaiseCanExecuteChanged();
        }

        private readonly DelegateCommand _prevItemCommand;
        public ICommand PrevItemCommand => _prevItemCommand;

        private readonly DelegateCommand _nextItemCommand;
        public ICommand NextItemCommand => _nextItemCommand;
    }
}
