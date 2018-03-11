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
using System.Threading.Tasks;
using System.Windows.Input;

using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.ButtonDriver.Macros
{
    [Export("ExecuteMacroCommand", typeof(ICommand))]
    internal class ExecuteMacroCommand : ICommand
    {
        private IButtonDriver _driver;
        private Task _currentTask;

        [Import]
        public IButtonDriver Driver
        {
            get => _driver;
            set
            {
                if (ReferenceEquals(_driver, value)) return;

                if (_driver != null) _driver.PropertyChanged -= Driver_StatusChanged;

                _driver = value;
                _driver.PropertyChanged += Driver_StatusChanged;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Driver_StatusChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            if (_currentTask != null && !_currentTask.IsCompleted) return false;
            if (_driver == null) return false;

            var macro = parameter as IMacro;
            return macro?.CanExecute(_driver) ?? false;
        }

        public virtual async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                var macro = (IMacro)parameter;
                _currentTask = macro.ExecuteAsync(_driver);
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await _currentTask;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to execute macro - {ex}");
            }
            finally
            {
                _currentTask = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
