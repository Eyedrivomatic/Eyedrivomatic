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

#include "IDynavoxHost.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

struct tobiigaze_eye_tracker;
struct GazeDataCallbackData;

DEFAULT_NAMESPACE{

	private ref class GazeDataSource : public IObservable<GazeData^>
	{
	public:
		GazeDataSource(tobiigaze_eye_tracker *eyeTracker, Logger *logger);
		virtual ~GazeDataSource();

		virtual IDisposable ^Subscribe(IObserver<GazeData^> ^observer);

	internal:
		void Unsubscribe(IObserver<GazeData^> ^observer);
		void OnGazeData(GazeData^ gazeData);

	private:
		void StartDataSource();
		void StopDataSource();

		List<IObserver<GazeData^>^> ^_observers;
		tobiigaze_eye_tracker *_eyeTracker;
		Logger *_logger;
		
		GCHandle _selfHandle;
	};

}END_DEFAULT_NAMESPACE
