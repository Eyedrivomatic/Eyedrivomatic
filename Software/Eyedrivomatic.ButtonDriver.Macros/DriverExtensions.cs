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
using Eyedrivomatic.ButtonDriver.Device.Services;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.ButtonDriver.Macros
{
    public static class DriverExtensions
    {
        internal static bool CanExecuteTask(this IButtonDriver driver, MacroTask task)
        {
            var driverTask = task as IButtonDriverMacroAsyncTask;
            if (driverTask == null) return true;

            return driverTask.CanExecute(driver);
        }

        internal static Task ExecuteTaskAsync(this IButtonDriver driver, MacroTask task)
        {
            var driverTask = task as IButtonDriverMacroAsyncTask;
            if (driverTask == null) return Task.FromResult(0);

            if (!driverTask.CanExecute(driver))
            {
                Log.Error(typeof(DriverExtensions), "Unable to execute task - task unavailable.");
                return Task.FromException(new InvalidOperationException("task unavailable"));
            }

            return driverTask.ExecuteAsync(driver);
        }
    }
}
