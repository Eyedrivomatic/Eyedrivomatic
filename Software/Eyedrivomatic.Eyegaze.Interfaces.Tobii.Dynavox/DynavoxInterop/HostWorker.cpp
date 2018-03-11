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


#include "stdafx.h"
#include "HostWorker.h"

#include "tobiigaze.h"

DEFAULT_NAMESPACE{

using namespace System;

HostWorker::HostWorker(Logger  *logger)
{
	_logger = logger;
}

HostWorker::~HostWorker()
{
	Stop();
	_logger = nullptr;
}

void TOBIIGAZE_CALL ThreadStoppedCallback(tobiigaze_error_code error_code, void  *user_data)
{
	auto logger = static_cast<Logger*>(user_data);
	logger->WriteLog(LogLevel::Diagnostic, "Dynavox worker thead stopped.");
}

void HostWorker::Start(tobiigaze_eye_tracker  *eyeTracker)
{
	_eyeTracker = eyeTracker;
	tobiigaze_run_event_loop_on_internal_thread(_eyeTracker, &ThreadStoppedCallback, _logger);
}

void HostWorker::Stop()
{
	if (_eyeTracker != NULL)
	{
		_logger->WriteLog(LogLevel::Diagnostic, "Stopping Dynavox worker thead.");
		tobiigaze_break_event_loop(_eyeTracker);
	}
	_eyeTracker = NULL;
}

}END_DEFAULT_NAMESPACE