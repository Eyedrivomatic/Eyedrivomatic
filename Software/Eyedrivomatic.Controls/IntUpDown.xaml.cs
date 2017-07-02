using System.Windows;
using System.Windows.Input;
using Prism.Commands;

namespace Eyedrivomatic.Controls
{
    public partial class IntUpDown
    {
        public IntUpDown()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(IntUpDown), new PropertyMetadata(default(int)));

        public int Value
        {
            get => (int) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue), typeof(int), typeof(IntUpDown), new PropertyMetadata(int.MaxValue));

        public int MaxValue
        {
            get => (int) GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue), typeof(int), typeof(IntUpDown), new PropertyMetadata(int.MinValue));

        public int MinValue
        {
            get => (int) GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public ICommand IncrementCommand => new DelegateCommand(() => Value++, () => Value < MaxValue);
        public ICommand DecrementCommand => new DelegateCommand(() => Value--, () => Value > MinValue);
    }
}
