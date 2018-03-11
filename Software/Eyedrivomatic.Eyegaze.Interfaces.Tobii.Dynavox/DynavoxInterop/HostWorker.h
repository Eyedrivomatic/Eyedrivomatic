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
#include "Logger.h"

#include "tobiigaze.h"

DEFAULT_NAMESPACE{

private class HostWorker
{
public:
	HostWorker(Logger *logger);
	~HostWorker();

	void Start(tobiigaze_eye_tracker *tracker);
	void Stop();

private:
	Logger *_logger;
	tobiigaze_eye_tracker *_eyeTracker;
};

}END_DEFAULT_NAMESPACE
