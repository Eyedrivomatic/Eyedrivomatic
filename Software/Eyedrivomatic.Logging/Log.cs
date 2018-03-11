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
using log4net;

namespace Eyedrivomatic.Logging
{
    public static class Log
    {
        public static void Initialize()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void Debug(Type context, string message)
        {
            var logger = LogManager.GetLogger(context);
            logger.Debug(message);
        }

        public static void Info(Type context, string message)
        {
            var logger = LogManager.GetLogger(context);
            logger.Info(message);
        }

        public static void Warn(Type context, string message)
        {
            var logger = LogManager.GetLogger(context);
            logger.Warn(message);
        }

        public static void Error(Type context, string message)
        {
            var logger = LogManager.GetLogger(context);
            logger.Error(message);
        }


        public static void Debug(object context, string message)
        {
            Debug(context.GetType(), message);
        }

        public static void Info(object context, string message)
        {
            Info(context.GetType(), message);
        }

        public static void Warn(object context, string message)
        {
            Warn(context.GetType(), message);
        }

        public static void Error(object context, string message)
        {
            Error(context.GetType(), message);
        }
    }
}
