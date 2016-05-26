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


using System.Diagnostics;

using Prism.Logging;


namespace Eyedrivomatic
{
    /// <summary>
    /// Provides an ILoggerFacade for the log4net logger.
    /// Uses the calling method type to get the type-specific log4net logger.
    /// </summary>
    public class Log4NetLogger : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            var callerType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;

            switch (category)
            {
                case Category.Debug:
                    Infrastructure.Log.Debug(callerType, message);
                    break;
                case Category.Info:
                    Infrastructure.Log.Info(callerType, message);
                    break;
                case Category.Warn:
                    Infrastructure.Log.Warn(callerType, message);
                    break;
                case Category.Exception:
                    Infrastructure.Log.Error(callerType, message);
                    break;
            }
        }
    }
}
