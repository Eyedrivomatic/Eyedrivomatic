using System;
using System.IO;
using log4net;
using log4net.Config;
using Prism.Logging;

namespace Eyedrivomatic.Eyegaze.UnitTests
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
        private readonly ILog _logger;

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
