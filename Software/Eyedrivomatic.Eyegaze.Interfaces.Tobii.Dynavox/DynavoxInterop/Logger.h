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


#pragma once

#include <vcclr.h>  

#include "tobiigaze.h"

using namespace System;

DEFAULT_NAMESPACE{ 
	
	public enum class LogLevel
	{
		Diagnostic,
		Information,
		Warning,
		Error
	};
	
	public delegate void LogWriter(LogLevel logLevel, String ^message);

	private class Logger
	{
	public:
		Logger(LogWriter ^logWriter);
		~Logger();

		void Register(tobiigaze_eye_tracker *eyeTracker);
		void WriteLog(LogLevel level, String ^message);

	private:
		gcroot<LogWriter^> _logWriter;
	};

}END_DEFAULT_NAMESPACE
