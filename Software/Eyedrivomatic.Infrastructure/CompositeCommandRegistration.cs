using System;
using System.Linq;
using System.Windows.Input;
using NullGuard;
using Prism.Commands;

namespace Eyedrivomatic.Infrastructure
{
    public sealed class CompositeCommandRegistration : IDisposable
    {
        private readonly CompositeCommand _compositeCommand;
        private readonly ICommand _targetCommand;

        public CompositeCommandRegistration(CompositeCommand compositeCommand, ICommand targetCommand)
        {
            _compositeCommand = compositeCommand;
            _targetCommand = targetCommand;
            _compositeCommand.RegisterCommand(_targetCommand);
        }

        public void Dispose()
        {
            _compositeCommand.UnregisterCommand(_targetCommand);
        }
    }

    public static class CompositeCommandExtensions
    {
        public static IDisposable DisposableRegisterCommand(
            this CompositeCommand compositeCommand,
            ICommand targetCommand)
        {
            return new CompositeCommandRegistration(compositeCommand, targetCommand);
        }
    }

    /// <summary>
    /// Like the CompositeCommand, but can execute when ANY of the 
    /// registered commands can execute.
    /// </summary>
    public class CompositeCommandAny : CompositeCommand
    {
        public override bool CanExecute([AllowNull] object parameter)
        {
            return base.CanExecute(parameter) ||
            RegisteredCommands.Any(c => c.CanExecute(parameter));
        }

        public override void Execute([AllowNull] object parameter)
        {
            base.Execute(parameter);
        }
    }
}
