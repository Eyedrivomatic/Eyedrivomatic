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
using System.IO;
using log4net;
using log4net.Config;
using Prism.Logging;

namespace Eyedrivomatic.Controls.UnitTests
{
    public class TestLogging
    {
        static TestLogging()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.properties"));
        }

        public static ILoggerFacade GetLogger(Type context)
        {
            var logger = LogManager.GetLogger(context);
            return new Log4NetLogger(logger);
        }
    }

    public class Log4NetLogger : ILoggerFacade
    {
        private ILog _logger;

        public Log4NetLogger(ILog logger)
        {
            _logger = logger;
        }

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    _logger.Debug(message);
                    break;
                case Category.Info:
                    _logger.Info(message);
                    break;
                case Category.Warn:
                    _logger.Warn(message);
                    break;
                case Category.Exception:
                    _logger.Error(message);
                    break;
            }
        }
    }
}
