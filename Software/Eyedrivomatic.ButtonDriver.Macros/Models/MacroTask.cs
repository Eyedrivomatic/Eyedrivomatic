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
using System.ComponentModel;
using System.Linq;

using Eyedrivomatic.Resources;
using System.Xml.Serialization;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    /// <summary>
    /// The base class for an Eyedrivomatic macro.
    /// </summary>
    public abstract class MacroTask : IDataErrorInfo, IEquatable<MacroTask>
    {
        /// <summary>
        /// The name of the node as it should be seen by the user.
        /// </summary>
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Returns a string that describes the expected result of executing the task.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        #region IDataErrorInfo
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string propertyName] => GetValidationError(propertyName);
        #endregion IDataErrorInfo

        #region IEquatable
        public abstract bool Equals(MacroTask other);
        #endregion IEquatable

        #region Validation

        protected abstract string[] ValidatedProperties { get; }

        protected virtual string GetValidationError(string propertyName)
        {
            return propertyName == nameof(DisplayName) ? ValidateDisplayName() : null;
        }

        private string ValidateDisplayName()
        {
            return string.IsNullOrWhiteSpace(DisplayName) ? Strings.MacroTask_InvalidDisplayName : null;
        }

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid => ValidatedProperties.Any(propertyName => GetValidationError(propertyName) != null);
        #endregion Validation
    }
}
