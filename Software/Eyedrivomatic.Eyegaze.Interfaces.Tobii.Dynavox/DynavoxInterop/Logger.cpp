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
#include "Logger.h"

DEFAULT_NAMESPACE{

	Logger::Logger(LogWriter ^logWriter)
	{
		_logWriter = logWriter;
	}

	Logger::~Logger()
	{
		_logWriter = nullptr;
	}

	void TOBIIGAZE_CALL ErrorCallback(tobiigaze_error_code errorCode, void *user_data)
	{
		auto self = static_cast<Logger*>(user_data);
		self->WriteLog(LogLevel::Error, String::Format("Tobii Dynavox device reported an error - [{0}]", gcnew String(tobiigaze_get_error_message(errorCode))));
	}

	void Logger::Register(tobiigaze_eye_tracker  *eyeTracker)
	{
		tobiigaze_register_error_callback(eyeTracker, &ErrorCallback, this);
	}


	void Logger::WriteLog(LogLevel level, String ^message)
	{
		if (System::Object::ReferenceEquals(_logWriter, nullptr)) return;
		_logWriter->BeginInvoke(level, message, nullptr, nullptr);
	}

}END_DEFAULT_NAMESPACE
