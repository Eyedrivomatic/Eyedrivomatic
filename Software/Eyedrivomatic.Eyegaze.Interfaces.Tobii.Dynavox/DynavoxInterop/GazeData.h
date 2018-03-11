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

DEFAULT_NAMESPACE{

	public value class Point2D
	{
	public:
		Point2D(double x, double y)
			: X(x), Y(y)
		{}

		double X;
		double Y;
	};

	public ref class GazeData
	{
	public:
		enum class TrackingStatus
		{
			NoEyesTracked = TOBIIGAZE_TRACKING_STATUS_NO_EYES_TRACKED,
			BothEyesTracked = TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED,
			OnlyLeftEyeTracked = TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED,
			OneEyeTrackedProbablyLeft = TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_LEFT,
			OneEyeTrackedUnknownWhich = TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_UNKNOWN_WHICH,
			OneEyeTrackedProbablyRight = TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_RIGHT,
			OnlyRightEyeTracked = TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED
		};

		property TrackingStatus Status { TrackingStatus get() { return _status; } }
		property Point2D GazePointLeft { Point2D get() { return _gazePointLeft; } }
		property Point2D GazePointRight { Point2D get() { return _gazePointRight; } }

	internal:
		GazeData(TrackingStatus status, Point2D gazePointLeft, Point2D gazePointRight)
			: _status(status), _gazePointLeft(gazePointLeft), _gazePointRight(gazePointRight)
		{}

	private:
		TimeSpan _age;
		TrackingStatus _status;
		Point2D _gazePointLeft;
		Point2D _gazePointRight;
	};

}END_DEFAULT_NAMESPACE