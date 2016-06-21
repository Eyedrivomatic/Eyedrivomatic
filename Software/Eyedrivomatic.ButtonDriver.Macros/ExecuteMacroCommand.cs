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
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Logging;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Macros.Models;

namespace Eyedrivomatic.ButtonDriver.Macros
{
    [Export("ExecuteMacroCommand")]
    public class ExecuteMacroCommand : ICommand
    {
        private IButtonDriver _driver;
        private Task _currentTask;

        [Import]
        public IButtonDriver Driver
        {
            get { return _driver; }
            set
            {
                if (object.ReferenceEquals(_driver, value)) return;
                _driver = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            if (!_currentTask?.IsCompleted ?? false) return false;
            if (_driver == null) return false;

            var macro = parameter as IMacro;
            if (macro == null) return false;

            return macro.CanExecute(_driver);
        }

        public async virtual void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                var macro = (IMacro)parameter;
                _currentTask = macro.ExecuteAsync(_driver);
                await _currentTask;
            }
            catch (Exception ex)
            {
                MacrosModule.Logger?.Log($"Failed to execute macro - {ex}", Category.Exception, Priority.None);
            }
            finally
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
