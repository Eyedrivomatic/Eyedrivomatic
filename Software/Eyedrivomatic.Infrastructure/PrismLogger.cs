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


using System.Diagnostics;
using Prism.Logging;

namespace Eyedrivomatic.Infrastructure
{
    /// <summary>
    /// Provides an ILoggerFacade for the log4net logger.
    /// Uses the calling method type to get the type-specific log4net logger.
    /// </summary>
    public class PrismLogger : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            var callerType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;

            switch (category)
            {
                case Category.Debug:
                    Logging.Log.Debug(callerType, message);
                    break;
                case Category.Info:
                    Logging.Log.Info(callerType, message);
                    break;
                case Category.Warn:
                    Logging.Log.Warn(callerType, message);
                    break;
                case Category.Exception:
                    Logging.Log.Error(callerType, message);
                    break;
            }
        }
    }
}
