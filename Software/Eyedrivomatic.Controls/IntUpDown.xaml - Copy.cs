using System;
using System.Windows;
using System.Windows.Input;
using Eyedrivomatic.Infrastructure;
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
            nameof(Value), typeof(int), typeof(IntUpDown), new PropertyMetadata(default(int), ValuePropertyChanged, CoerceValue), ValidateValueCallback);

        protected static void UpdateMetadata( Type type, int? increment, int? minValue, int? maxValue )
        {
            DefaultStyleKeyProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( type ) );
            UpdateMetadataCommon( type, increment, minValue, maxValue );
        }

        private static void UpdateMetadataCommon( Type type, T? increment, T? minValue, T? maxValue )
        {
            IncrementProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( increment ) );
            MaximumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( maxValue ) );
            MinimumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( minValue ) );
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            var ud = d as IntUpDown;
            if (ud == null) return basevalue;

            try
            {
                var intVal = Convert.ToInt32(basevalue);
                return Math.Min(Math.Max(intVal, ud.MinValue), ud.MaxValue);
            }
            catch (OverflowException)
            {
                Log.Warn(typeof(IntUpDown), $"The {basevalue.GetType().Name} value {basevalue} is outside the range of the Int32 type.");
            }
            catch (FormatException)
            {
                Log.Warn(typeof(IntUpDown), $"The {basevalue.GetType().Name} value {basevalue} is not in a recognizable format.");
            }
            catch (InvalidCastException)
            {
                Log.Warn(typeof(IntUpDown), $"No conversion to an Int32 exists for the {basevalue.GetType().Name} value {basevalue}.");
            }
            return null;
        }

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ud = d as IntUpDown;
            if (ud == null) return;
            ud.IncrementCommand.RaiseCanExecuteChanged();
            ud.DecrementCommand.RaiseCanExecuteChanged();
        }

        private static bool ValidateValueCallback(object value)
        {
            try
            {
                var intVal = Convert.ToInt32(value);
            }
            catch (OverflowException)
            {
                Log.Warn(typeof(IntUpDown), $"The {value.GetType().Name} value {value} is outside the range of the Int32 type.");
            }
            catch (FormatException)
            {
                Log.Warn(typeof(IntUpDown), $"The {value.GetType().Name} value {value} is not in a recognizable format.");
            }
            catch (InvalidCastException)
            {
                Log.Warn(typeof(IntUpDown), $"No conversion to an Int32 exists for the {value.GetType().Name} value {value}.");
            }
            return false;
        }

        public int Value
        {
            get => (int)(GetValue(ValueProperty) ?? 0);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue), typeof(int), typeof(IntUpDown), new PropertyMetadata(int.MaxValue));

        public int MaxValue
        {
            get => (int)(GetValue(MaxValueProperty) ?? int.MaxValue);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue), typeof(int), typeof(IntUpDown), new PropertyMetadata(int.MinValue));

        public int MinValue
        {
            get => (int)(GetValue(MinValueProperty) ?? int.MinValue);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step), typeof(int), typeof(IntUpDown), new PropertyMetadata(1));

        public int Step
        {
            get => (int)(GetValue(StepProperty) ?? 1);
            set => SetValue(StepProperty, value);
        }

        public DelegateCommand IncrementCommand => new DelegateCommand(() => Value += Step, () => Value <= MaxValue + Step);
        public DelegateCommand DecrementCommand => new DelegateCommand(() => Value -= Step, () => Value >= MinValue - Step);

    }
}
