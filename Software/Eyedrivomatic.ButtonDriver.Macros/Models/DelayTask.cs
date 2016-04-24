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


using Eyedrivomatic.Hardware;
using System;
using System.Threading.Tasks;

namespace Eyedrivomatic.Modules.Macros.Models
{
    public class DelayTask : MacroTask, IMacroAsyncTask
    {
        public static DelayTask CreateNew(IDriver driver)
        {
            return new DelayTask
            {
                DelayTime = TimeSpan.FromSeconds(1)
            };
        }

        private DelayTask() {} //Hide the constructor.

        public TimeSpan DelayTime { get; set; }

        public bool CanExecute()
        {
            return DelayTime > TimeSpan.Zero;
        }

        public Task ExecuteAsync()
        {
            return Task.Delay(DelayTime);
        }

        public override string ToString()
        {
            return string.Format(Strings.DelayTask_ToStringFormat, DelayTime.TotalSeconds);
        }

        #region Validation

        protected override string[] ValidatedProperties => new[] { nameof(DelayTime) };

        protected override string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0) return null;

            switch (propertyName)
            {
                case nameof(DelayTime):
                    return ValidateDelayTime();
            }

            return base.GetValidationError(propertyName);
        }

        string ValidateDelayTime()
        {
            if (DelayTime <= TimeSpan.Zero) return string.Format(Strings.ToggleRelayMacroTask_InvalidRelay, DelayTime);
            return null;
        }

        #endregion // Validation
    }
}
