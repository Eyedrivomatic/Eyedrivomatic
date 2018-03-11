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

#include "GazeDataSource.h"
#include "GazeDataRegistration.h"

#include "tobiigaze.h"

#include <vcclr.h>  

DEFAULT_NAMESPACE{

	GazeDataSource::GazeDataSource(tobiigaze_eye_tracker *eyeTracker, Logger *logger)
		: _eyeTracker(eyeTracker), _observers(gcnew List<IObserver<GazeData^>^>()), _logger(logger), _selfHandle(GCHandle::Alloc(this, GCHandleType::WeakTrackResurrection))
	{}

	GazeDataSource::~GazeDataSource()
	{
		_logger->WriteLog(LogLevel::Diagnostic, "Deleting GazeDataSource.");

		if (_observers->Count > 0)
		{
			StopDataSource();
			for each (auto observer in _observers)
			{
				observer->OnCompleted();
			}
			_observers->Clear();
		}
		_observers = nullptr;
		_eyeTracker = NULL;
		_selfHandle.Free();
	}

	IDisposable ^GazeDataSource::Subscribe(IObserver<GazeData^> ^observer)
	{
		_logger->WriteLog(LogLevel::Diagnostic, "Observer subscribing to GazeData.");

		_observers->Add(observer);
		if (_observers->Count == 1) StartDataSource();

		return gcnew GazeDataRegistration(this, observer, _logger);
	}

	void GazeDataSource::Unsubscribe(IObserver<GazeData^> ^observer)
	{
		if (!_observers->Contains(observer)) return;

		_logger->WriteLog(LogLevel::Diagnostic, "Observer unsubscribing from GazeData.");

		_observers->Remove(observer);
		observer->OnCompleted();

		if (_observers->Count == 0) StopDataSource();
	}

	void start_tracking_callback(tobiigaze_error_code error_code, void *user_data)
	{
		auto logger = static_cast<Logger*>(user_data);
		logger->WriteLog(LogLevel::Diagnostic, "GazeData tracking started.");
	}

	void stop_tracking_callback(tobiigaze_error_code error_code, void *user_data)
	{
		auto logger = static_cast<Logger*>(user_data);
		logger->WriteLog(LogLevel::Diagnostic, "GazeData tracking stopped.");
	}

	void GazeDataSource::OnGazeData(GazeData^ data)
	{
		_logger->WriteLog(LogLevel::Diagnostic, String::Format("Gaze data received [{0}] Left:[{1}, {2}], Right:[{3}, {4}].", 
			data->Status, data->GazePointLeft.X, data->GazePointLeft.Y, data->GazePointRight.X, data->GazePointRight.Y));

		for each (auto observer in _observers)
		{
			observer->OnNext(data);
		}
	}

	void on_gaze_data(const tobiigaze_gaze_data* gazedata, const tobiigaze_gaze_data_extensions* extensions, void *user_data)
	{
		auto handle = GCHandle::FromIntPtr(IntPtr(user_data));
		if (handle.Target == nullptr) return;
		auto dataSource = dynamic_cast<GazeDataSource^>(handle.Target);
		if (dataSource == nullptr) return;

		dataSource->OnGazeData(gcnew GazeData(
			(GazeData::TrackingStatus)gazedata->tracking_status, 
			Point2D(gazedata->left.gaze_point_on_display_normalized.x, gazedata->left.gaze_point_on_display_normalized.y),
			Point2D(gazedata->right.gaze_point_on_display_normalized.x, gazedata->right.gaze_point_on_display_normalized.y)));
	}

	void GazeDataSource::StartDataSource()
	{
		auto selfPtr = GCHandle::ToIntPtr(_selfHandle).ToPointer();
		tobiigaze_start_tracking_async(_eyeTracker, &start_tracking_callback, &on_gaze_data, selfPtr);
	}

	void GazeDataSource::StopDataSource()
	{
		tobiigaze_stop_tracking_async(_eyeTracker, &stop_tracking_callback, _logger);
	}

}END_DEFAULT_NAMESPACE
