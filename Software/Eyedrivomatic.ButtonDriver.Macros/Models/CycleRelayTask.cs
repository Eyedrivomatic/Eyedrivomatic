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
using System.Threading.Tasks;
using Eyedrivomatic.Resources;
using System.Xml.Serialization;
using Eyedrivomatic.ButtonDriver.Hardware.Services;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [XmlType("CycleeRelay")]
    public class CycleRelayTask : MacroTask, IButtonDriverMacroAsyncTask
    {
        /// <summary>
        /// The relay to cycle
        /// </summary>
        [XmlAttribute("Relay")]
        public uint Relay { get; set; }

        /// <summary>
        /// The number of times to cycle the relay.
        /// </summary>
        [XmlAttribute("Repeat")]
        public uint Repeat { get; set; } = 1;

        /// <summary>
        /// The delay between on and off in milliseconds.
        /// </summary>
        [XmlAttribute("ToggleDelay")]
        public uint ToggleDelayMs { get; set; } = 500;

        /// <summary>
        /// The delay between cycle repeats in milliseconds.
        /// </summary>
        [XmlAttribute("CycleDelay")]
        public uint CycleDelayMs { get; set; } = 1000;

        
        #region IButtonDriverMacroTask
        public virtual async Task ExecuteAsync(IButtonDriver driver)
        {
            await driver.CycleRelayAsync(Relay, Repeat, ToggleDelayMs, CycleDelayMs);
        }

        public virtual bool CanExecute(IButtonDriver driver)
        {
            return driver != null && driver.ReadyState != ReadyState.None && Relay <= driver.RelayCount && driver.CurrentDirection == Direction.None;
        }
        #endregion IButtonDriverMacroTask

        public override string ToString()
        {
            return string.Format(Strings.CycleRelayMacroTask_ToStringFormat, Relay, Repeat, ToggleDelayMs);
        }

        #region IComparable
        public override bool Equals(MacroTask other)
        {
            var that = other as CycleRelayTask;

            return that != null
                && Relay == that.Relay
                && Repeat == that.Repeat
                && ToggleDelayMs == that.ToggleDelayMs;
        }
        #endregion IComparable

        #region Validation

        protected override string[] ValidatedProperties => new[] { nameof(Relay), nameof(Repeat), nameof(ToggleDelayMs) };

        protected override string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0) return null;

            switch (propertyName)
            {
                case nameof(Relay):
                    return ValidateRelay();
                case nameof(Repeat):
                    return ValidateRepeat();
                case nameof(ToggleDelayMs):
                    return ValidateToggleDelayMs();
                case nameof(CycleDelayMs):
                    return ValidateCycleDelayMs();
            }

            return base.GetValidationError(propertyName);
        }

        private string ValidateRelay()
        {
            if (Relay == 0 || Relay > ElectronicHandButtonDriver.AvailableRelays) return string.Format(Strings.CycleRelayMacroTask_InvalidRelay, ElectronicHandButtonDriver.AvailableRelays);
            return null;
        }

        private string ValidateRepeat()
        {
            if (Repeat == 0) return string.Format(Strings.CycleRelayMacroTask_InvalidRepeat);
            return null;
        }

        string ValidateToggleDelayMs()
        {
            return null;
        }

        string ValidateCycleDelayMs()
        {
            return null;
        }

        #endregion // Validation
    }
}
