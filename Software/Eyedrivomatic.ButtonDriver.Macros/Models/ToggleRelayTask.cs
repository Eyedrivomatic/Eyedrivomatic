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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Prism.Logging;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    public class ToggleRelayTask : MacroTask, IButtonDriverMacroAsyncTask
    {
        public static ToggleRelayTask CreateNew(IButtonDriver driver)
        {
            Contract.Requires<ArgumentNullException>(driver != null, nameof(driver));
            return new ToggleRelayTask(1, 1, 0, driver.RelayCount);
        }

        private readonly uint _deviceRelays;

        private ToggleRelayTask(uint relay, uint repeat, uint delayMs, uint deviceRelays)
        {
            Contract.Requires<ArgumentOutOfRangeException>(deviceRelays > 0, nameof(deviceRelays));
            Contract.Requires<ArgumentOutOfRangeException>(repeat > 0, nameof(repeat));
            Contract.Requires<ArgumentOutOfRangeException>(deviceRelays > 0, nameof(deviceRelays));
            Contract.Requires<ArgumentOutOfRangeException>(deviceRelays >= relay, "relay must be less than or equal to deviceRelays");

            Relay = relay;
            Repeat = repeat;
            DelayMs = delayMs;
            _deviceRelays = deviceRelays;
        }

        /// <summary>
        /// The relay to toggle
        /// </summary>
        public uint Relay { get; set; }

        /// <summary>
        /// The number of times to toggle the relay.
        /// </summary>
        public uint Repeat { get; set; }

        /// <summary>
        /// The delay between toggle repeats in milliseconds.
        /// </summary>
        public uint DelayMs { get; set; }

        #region IButtonDriverMacroTask
        public virtual async Task ExecuteAsync(IButtonDriver driver)
        {
            MacrosModule.Logger?.Log($"Executing step '{ToString()}'", Category.Info, Priority.None);

            await driver.ToggleRelayAsync(Relay, Repeat, DelayMs);

            MacrosModule.Logger?.Log($"Step '{ToString()}' completed", Category.Debug, Priority.None);
        }

        public virtual bool CanExecute(IButtonDriver driver)
        {
            return driver?.ReadyState == ReadyState.Any && Relay <= driver.RelayCount;
        }
        #endregion IButtonDriverMacroTask

        public override string ToString()
        {
            return string.Format(Strings.ToggleRelayMacroTask_ToStringFormat, Relay, Repeat, DelayMs);
        }

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
            if (Relay == 0 || Relay > _deviceRelays) return string.Format(Strings.ToggleRelayMacroTask_InvalidRelay, _deviceRelays);
            return null;
        }

        string ValidateRepeat()
        {
            if (Repeat == 0) return string.Format(Strings.ToggleRelayMacroTask_InvalidRepeat);
            return null;
        }

        string ValidateDelayMs()
        {
            return null;
        }

        #endregion // Validation
    }
}
