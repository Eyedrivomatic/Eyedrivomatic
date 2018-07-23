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


// LoggerService.h
#pragma once

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#define LOG_QUEUE_SIZE 2048

enum LogSeverity
{
	Debug,
	Info,
	Warn,
	Error,
	None
};

class LoggerServiceClass
{
 public:
	 LogSeverity getLogLevel();
	 void setLogLevel(LogSeverity minSeverity);
	 
	void debug(const char *message, ...);
	void debug_P(const char *message, ...);
	void info(const char *message, ...);
	void info_P(const char *message, ...);
	void warn(const char *message, ...);
	void warn_P(const char *message, ...);
	void error(const char *message, ...);
	void error_P(const char *message, ...);

	const char * severityToName(LogSeverity severity);
	LogSeverity serverityFromName(const char *name);

	void shouldQueueLogs(bool should);
	void sendQueuedLogs();

private:
	void internalSend(const char * prefix, const char * message, va_list ap);
	void internalSend_P(const char * prefix, const char * message, va_list ap);

	void queueLog(const char * log);

	char _queueBuffer[LOG_QUEUE_SIZE];
	volatile size_t _queueStart = 0;
	volatile size_t _queueEnd = 0;
	bool _shouldQueueLogs = true;

#ifdef _DEBUG
	LogSeverity _minSeverity = LogSeverity::Debug;
#else
	LogSeverity _minSeverity = LogSeverity::Warn;
#endif
};

extern LoggerServiceClass LoggerService;

