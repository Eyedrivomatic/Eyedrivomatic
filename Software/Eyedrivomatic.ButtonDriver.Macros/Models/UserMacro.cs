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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Serialization;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [XmlType(TypeName = "Macro")]
    public class UserMacro : IMacro, IEquatable<IMacro>
    {
        [XmlAttribute(AttributeName ="Icon")]
        public string IconPath { get; set; }

        [XmlAttribute(AttributeName ="DisplayName")]
        public string DisplayName { get; set; }

        [XmlIgnore]
        public bool IsExecuting { get; private set; }

        [XmlArrayItem(Type = typeof(CycleRelayTask), ElementName = "CycleRelay")]
        [XmlArrayItem(Type = typeof(DelayTask), ElementName = "Delay")]
        public ObservableCollection<MacroTask> Tasks { get; internal set; } = new ObservableCollection<MacroTask>();

        public async Task ExecuteAsync(IButtonDriver driver)
        {
            if (IsExecuting)
            {
                Log.Warn(this, $"Unable to execute macro '{DisplayName}'. Macro is currently running.");
            }

            try
            {
                IsExecuting = true;
                Log.Info(this, $"Executing macro '{DisplayName}'");

                foreach (var task in Tasks)
                {
                    await driver.ExecuteTaskAsync(task);
                }

                Log.Info(this, $"Macro '{DisplayName}' complete.");
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Macro '{DisplayName}' Failed - {ex}");
            }
            finally
            {
                IsExecuting = false;
            }
        }

        public bool CanExecute(IButtonDriver driver)
        {
            return !IsExecuting && Tasks.All(task => driver.CanExecuteTask(task));
        }

        #region IDataErrorInfo
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string propertyName] => GetValidationError(propertyName);
        #endregion IDataErrorInfo

        #region IEquatable 
        /// <summary>
        /// This is intended to assist with testing.
        /// </summary>
        public bool Equals(IMacro other)
        {
            return DisplayName == other.DisplayName &&
                Tasks.SequenceEqual<MacroTask>(other.Tasks);
        }
        #endregion IEquatable

        #region Validation

        protected string[] ValidatedProperties => new[] { nameof(DisplayName), nameof(Tasks) };

        protected string GetValidationError(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(DisplayName):
                    return ValidateDisplayName();
            }

            return null;
        }


        private string ValidateDisplayName()
        {
            if (string.IsNullOrWhiteSpace(DisplayName)) return Strings.Macro_InvalidDisplayName;
            return null;
        }

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid => ValidatedProperties.Any(propertyName => GetValidationError(propertyName) != null);
        #endregion Validation
    }
}
