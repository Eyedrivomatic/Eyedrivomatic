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
using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.Common;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [XmlType("CycleeSwitch")]
    public class CycleSwitchTask : MacroTask, IButtonDriverMacroAsyncTask
    {
        public const uint GenericDeviceMaxSwitchCount = 5;

        /// <summary>
        /// The relay to cycle
        /// </summary>
        [XmlAttribute("SwitchNumber")]
        public uint SwitchNumber { get; set; }

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
            await driver.CycleSwitchAsync(SwitchNumber, Repeat, ToggleDelayMs, CycleDelayMs);
        }

        public virtual bool CanExecute(IButtonDriver driver)
        {
            return driver != null && driver.ReadyState != ReadyState.None && SwitchNumber <= driver.SwitchCount && driver.CurrentDirection == Direction.None;
        }
        #endregion IButtonDriverMacroTask

        public override string ToString()
        {
            return string.Format(Strings.CycleSwitchMacroTask_ToStringFormat, SwitchNumber, Repeat, ToggleDelayMs);
        }

        #region IComparable
        public override bool Equals(MacroTask other)
        {
            var that = other as CycleSwitchTask;

            return that != null
                && SwitchNumber == that.SwitchNumber
                && Repeat == that.Repeat
                && ToggleDelayMs == that.ToggleDelayMs;
        }
        #endregion IComparable

        #region Validation

        protected override string[] ValidatedProperties => new[] { nameof(SwitchNumber), nameof(Repeat), nameof(ToggleDelayMs) };

        protected override string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0) return null;

            switch (propertyName)
            {
                case nameof(SwitchNumber):
                    return ValidateSwitch();
                case nameof(Repeat):
                    return ValidateRepeat();
                case nameof(ToggleDelayMs):
                    return ValidateToggleDelayMs();
                case nameof(CycleDelayMs):
                    return ValidateCycleDelayMs();
            }

            return base.GetValidationError(propertyName);
        }

        private string ValidateSwitch()
        {
            if (SwitchNumber == 0 || SwitchNumber > GenericDeviceMaxSwitchCount) return string.Format(Strings.CycleSwitchMacroTask_InvalidSwitch, GenericDeviceMaxSwitchCount);
            return null;
        }

        private string ValidateRepeat()
        {
            if (Repeat == 0) return string.Format(Strings.CycleSwitchMacroTask_InvalidRepeat);
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
