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
using System.Threading.Tasks;

using Prism.Logging;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Resources;
using System.Xml.Serialization;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [XmlType("CycleeRelay")]
    public class CycleRelayTask : MacroTask, IButtonDriverMacroAsyncTask
    {
        /// <summary>
        /// The relay to toggle
        /// </summary>
        [XmlAttribute("Relay")]
        public uint Relay { get; set; }

        /// <summary>
        /// The number of times to toggle the relay.
        /// </summary>
        [XmlAttribute("Repeat")]
        public uint Repeat { get; set; }

        /// <summary>
        /// The delay between toggle repeats in milliseconds.
        /// </summary>
        [XmlAttribute("Delay")]
        public uint DelayMs { get; set; }

        #region IButtonDriverMacroTask
        public virtual async Task ExecuteAsync(IButtonDriver driver)
        {
            MacrosModule.Logger?.Log($"Executing step '{ToString()}'", Category.Info, Priority.None);

            await driver.CycleRelayAsync(Relay, Repeat, DelayMs);

            MacrosModule.Logger?.Log($"Step '{ToString()}' completed", Category.Debug, Priority.None);
        }

        public virtual bool CanExecute(IButtonDriver driver)
        {
            return driver?.ReadyState == ReadyState.Any && Relay <= driver.RelayCount;
        }
        #endregion IButtonDriverMacroTask

        public override string ToString()
        {
            return string.Format(Strings.CycleRelayMacroTask_ToStringFormat, Relay, Repeat, DelayMs);
        }

        #region IComparable
        public override bool Equals(MacroTask other)
        {
            var that = other as CycleRelayTask;

            return that != null
                && this.Relay == that.Relay
                && this.Repeat == that.Repeat
                && this.DelayMs == that.DelayMs;
        }
        #endregion IComparable

        #region Validation

        protected override string[] ValidatedProperties => new[] { nameof(Relay), nameof(Repeat), nameof(DelayMs) };

        protected override string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0) return null;

            switch (propertyName)
            {
                case nameof(Relay):
                    return ValidateRelay();
                case nameof(Repeat):
                    return ValidateRepeat();
                case nameof(DelayMs):
                    return ValidateDelayMs();
            }

            return base.GetValidationError(propertyName);
        }

        string ValidateRelay()
        {
            if (Relay == 0 || Relay > BrainBoxDriver.AvailableRelays) return string.Format(Strings.CycleRelayMacroTask_InvalidRelay, BrainBoxDriver.AvailableRelays);
            return null;
        }

        string ValidateRepeat()
        {
            if (Repeat == 0) return string.Format(Strings.CycleRelayMacroTask_InvalidRepeat);
            return null;
        }

        string ValidateDelayMs()
        {
            return null;
        }

        #endregion // Validation
    }
}
