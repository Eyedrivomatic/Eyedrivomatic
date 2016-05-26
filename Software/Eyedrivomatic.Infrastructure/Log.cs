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
using log4net;

namespace Eyedrivomatic.Infrastructure
{
    public class Log
    {
        public static void Initialize()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void Debug(Type context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));

            var logger = LogManager.GetLogger(context);
            logger.Debug(message);
        }

        public static void Info(Type context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));

            var logger = LogManager.GetLogger(context);
            logger.Info(message);
        }

        public static void Warn(Type context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));

            var logger = LogManager.GetLogger(context);
            logger.Warn(message);
        }

        public static void Error(Type context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));

            var logger = LogManager.GetLogger(context);
            logger.Error(message);
        }


        public static void Debug(object context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Log.Debug(context.GetType(), message);
        }

        public static void Info(object context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Log.Info(context.GetType(), message);
        }

        public static void Warn(object context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Log.Warn(context.GetType(), message);
        }

        public static void Error(object context, string message)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Log.Warn(context.GetType(), message);
        }
    }
}
