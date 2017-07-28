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

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step), typeof(int), typeof(IntUpDown), new PropertyMetadata(1));

        public int Step
        {
            get => (int) GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public ICommand IncrementCommand => new DelegateCommand(() => Value += Step, () => Value <= MaxValue + Step);
        public ICommand DecrementCommand => new DelegateCommand(() => Value -= Step, () => Value >= MinValue - Step);

    }
}
