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

using namespace System;
//using namespace System::Threading::Tasks;

#include "Logger.h"
#include "GazeData.h"

DEFAULT_NAMESPACE{

	public interface class IDynavoxHost : public IDisposable
	{
		property bool IsInitialized { bool get(); }
		property bool IsConnected { bool get(); }
		property IObservable<GazeData^> ^DataStream { IObservable<GazeData^> ^get(); }
		
		bool Initialize(LogWriter ^loggingCallback);
		System::Threading::Tasks::Task<bool> ^InitializeAsync(LogWriter ^loggingCallback);
};

}END_DEFAULT_NAMESPACE
