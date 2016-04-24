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

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;


namespace Eyedrivomatic.Modules.Macros.Models
{
    /// <summary>
    /// The base class for an Eyedrivomatic macro.
    /// </summary>
    [ContractClass(typeof(Contracts.MacroTaskContract))]
    public abstract class MacroTask : IDataErrorInfo
    {
        protected MacroTask()
        {
        }

        /// <summary>
        /// The name of the node as it should be seen by the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Returns a string that describes the expected result of executing the task.
        /// </summary>
        /// <returns></returns>
        public override abstract string ToString();

        #region IDataErrorInfo
        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName] => GetValidationError(propertyName);
        #endregion IDataErrorInfo

        #region Validation

        protected abstract string[] ValidatedProperties { get; }

        protected virtual string GetValidationError(string propertyName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(propertyName), nameof(propertyName));

            switch (propertyName)
            {
                case nameof(DisplayName):
                    return ValidateDisplayName();
            }

            return null;
        }

        private string ValidateDisplayName()
        {
            if (string.IsNullOrWhiteSpace(DisplayName)) return Strings.MacroTask_InvalidDisplayName;
            return null;
        }

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid => ValidatedProperties.Any(propertyName => GetValidationError(propertyName) != null);
        #endregion Validation
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(MacroTask))]
        public abstract class MacroTaskContract : MacroTask
        {
            protected override string[] ValidatedProperties
            {
                get
                {
                    Contract.Ensures(Contract.Result<string[]>() != null);
                    throw new NotImplementedException();
                }
            }

            protected override abstract string GetValidationError(string propertyName);
        }
    }
}
