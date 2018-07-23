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


// 
// 
// 

#include "LoggerService.h"

#define LOG_BUFFER_SIZE 512

const char DebugLogPrefix[] PROGMEM = "LOG: DEBUG - ";
const char InfoLogPrefix[] PROGMEM = "LOG: INFO - ";
const char WarnLogPrefix[] PROGMEM = "LOG: WARN - ";
const char ErrorLogPrefix[] PROGMEM = "LOG: ERROR - ";

const char SeverityNameNone[] = "NONE";
const char SeverityNameDebug[] = "DEBUG";
const char SeverityNameInfo[] = "INFO";
const char SeverityNameWarn[] = "WARN";
const char SeverityNameError[] = "ERROR";

void LoggerServiceClass::setLogLevel(LogSeverity minSeverity)
{
	_minSeverity = minSeverity;
	debug_P(PSTR("Log Level set to %s"), severityToName(_minSeverity));
}

LogSeverity LoggerServiceClass::getLogLevel()
{
	return _minSeverity;
}

const char * LoggerServiceClass::severityToName(LogSeverity severity)
{
	switch (severity)
	{
	case LogSeverity::None:
		return SeverityNameNone;
	case LogSeverity::Debug:
		return SeverityNameDebug;
	case LogSeverity::Info:
		return SeverityNameInfo;
	case LogSeverity::Warn:
		return SeverityNameWarn;
	case LogSeverity::Error:
		return SeverityNameError;
	default:
		return SeverityNameNone;
	}
}

LogSeverity LoggerServiceClass::serverityFromName(const char *name)
{
	if (strcasecmp(name, SeverityNameDebug) == 0) return LogSeverity::Debug;
	if (strcasecmp(name, SeverityNameInfo) == 0) return LogSeverity::Info;
	if (strcasecmp(name, SeverityNameWarn) == 0) return LogSeverity::Warn;
	if (strcasecmp(name, SeverityNameError) == 0) return LogSeverity::Error;
	return LogSeverity::None;
}

void LoggerServiceClass::shouldQueueLogs(bool should)
{
	_shouldQueueLogs = should;
}

void LoggerServiceClass::sendQueuedLogs()
{
	while (_queueStart != _queueEnd)
	{
		Serial.write(_queueBuffer + _queueStart, 1);
		_queueStart = (_queueStart + 1) % LOG_QUEUE_SIZE;
	}
}

void LoggerServiceClass::queueLog(const char * log)
{
	while (*log != '\0')
	{
		_queueBuffer[_queueEnd] = *log++;
		_queueEnd = (_queueEnd + 1) % LOG_QUEUE_SIZE;
	}
}

void LoggerServiceClass::debug(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Debug) return;

	va_list vl;  
	va_start(vl, message);
	internalSend(DebugLogPrefix, message, vl);
}

void LoggerServiceClass::debug_P(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Debug) return;

	va_list vl;
	va_start(vl, message);
	internalSend_P(DebugLogPrefix, message, vl);
}

void LoggerServiceClass::info(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Info) return;

	va_list vl;
	va_start(vl, message);
	internalSend(InfoLogPrefix, message, vl);
}

void LoggerServiceClass::info_P(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Info) return;

	va_list vl;
	va_start(vl, message);
	internalSend_P(InfoLogPrefix, message, vl);
}

void LoggerServiceClass::warn(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Warn) return;

	va_list vl;
	va_start(vl, message);
	internalSend(WarnLogPrefix, message, vl);
}

void LoggerServiceClass::warn_P(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Warn) return;

	va_list vl;
	va_start(vl, message);
	internalSend_P(WarnLogPrefix, message, vl);
}

void LoggerServiceClass::error(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Error) return;

	va_list vl;
	va_start(vl, message);
	internalSend(ErrorLogPrefix, message, vl);
}

void LoggerServiceClass::error_P(const char *message, ...)
{
	if (_minSeverity > LogSeverity::Error) return;

	va_list vl;
	va_start(vl, message);
	internalSend_P(ErrorLogPrefix, message, vl);
}

void LoggerServiceClass::internalSend(const char * prefix, const char * message, va_list ap)
{
	char buffer[LOG_BUFFER_SIZE];
	int prefixLen = strlen_P(prefix);
	strcpy_P(buffer, prefix);
	char *startStr = buffer + prefixLen;

	int len = vsnprintf(startStr, sizeof(buffer) - prefixLen - 2, message, ap);
	if (len == prefixLen) return;

	buffer[prefixLen + len] = '\n';
	buffer[prefixLen + len + 1] = '\0';
	if (_shouldQueueLogs) queueLog(buffer);
	else Serial.write(buffer);
}


void LoggerServiceClass::internalSend_P(const char * prefix, const char * message, va_list ap)
{
	char buffer[LOG_BUFFER_SIZE];
	size_t prefixLen = strlen_P(prefix);
	strcpy_P(buffer, prefix);

	int len = vsnprintf_P(buffer + prefixLen, LOG_BUFFER_SIZE - prefixLen - 2, message, ap);
	if (len == 0) return;

	buffer[prefixLen + len] = '\n';
	buffer[prefixLen + len + 1] = '\0';

	if (_shouldQueueLogs) queueLog(buffer);
	else Serial.write(buffer);
}

LoggerServiceClass LoggerService;

