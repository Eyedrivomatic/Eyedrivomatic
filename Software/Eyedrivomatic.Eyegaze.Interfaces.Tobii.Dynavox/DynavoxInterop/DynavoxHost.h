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


// DynavoxWrapper.h

#pragma once

using namespace System;
//using namespace System::Threading::Tasks;

#include "Logger.h"
#include "HostWorker.h"

#include "IDynavoxHost.h"

struct tobiigaze_eye_tracker;

DEFAULT_NAMESPACE {

	ref class GazeDataSource;

	private ref class DynavoxHost : public IDynavoxHost
	{
	public:
		virtual ~DynavoxHost();
		
		virtual property bool IsInitialized { bool get() { return _eyeTracker != NULL; } }
		virtual property bool IsConnected { bool get(); }
		virtual property IObservable<GazeData^> ^DataStream { IObservable<GazeData^> ^get(); }

		virtual bool Initialize(LogWriter ^loggingCallback);
		virtual System::Threading::Tasks::Task<bool> ^InitializeAsync(LogWriter ^loggingCallback);

	internal:
		DynavoxHost(String ^url);
		!DynavoxHost();
		bool LogAndCheckError(tobiigaze_error_code errorCode, const char *operation);
		void LogAndFailIfError(tobiigaze_error_code errorCode, const char *operation);
		bool LogDeviceInfo();
		
	private:
		String ^ _url;
		tobiigaze_eye_tracker *_eyeTracker;
		HostWorker *_worker;
		Logger *_logger;
		GazeDataSource ^_gazeDataSource;
	};

}END_DEFAULT_NAMESPACE
