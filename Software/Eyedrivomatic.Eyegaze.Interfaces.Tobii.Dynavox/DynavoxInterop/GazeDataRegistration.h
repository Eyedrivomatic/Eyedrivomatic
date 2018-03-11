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

#include "GazeDataSource.h"

using namespace System;

DEFAULT_NAMESPACE{

	private ref class GazeDataRegistration : public IDisposable
	{
	public:
		GazeDataRegistration(GazeDataSource ^gazeDataSource, IObserver<GazeData^> ^observer, Logger *logger)
		{
			_gazeDataSource = gazeDataSource;
			_observer = observer;
			_logger = logger;
		}

		virtual ~GazeDataRegistration()
		{
			this->!GazeDataRegistration();
		}
	protected:
		!GazeDataRegistration()
		{
			if (_observer != nullptr)
			{
				_logger->WriteLog(LogLevel::Diagnostic, "Disposing GazeDataRegistration.");
				_gazeDataSource->Unsubscribe(_observer);
				_observer = nullptr;
				_gazeDataSource = nullptr;
			}
		}

	private:
		GazeDataSource ^ _gazeDataSource;
		IObserver<GazeData^> ^_observer;
		Logger *_logger;
	};

}END_DEFAULT_NAMESPACE