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

        private DelegateCommand _prevItemCommand;
        public ICommand PrevItemCommand => _prevItemCommand;

        private DelegateCommand _nextItemCommand;
        public ICommand NextItemCommand => _nextItemCommand;
    }
}
