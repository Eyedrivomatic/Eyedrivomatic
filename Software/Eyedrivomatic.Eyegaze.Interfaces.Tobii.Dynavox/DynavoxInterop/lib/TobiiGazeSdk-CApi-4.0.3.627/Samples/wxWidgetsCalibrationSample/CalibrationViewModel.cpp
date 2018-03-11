/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#include "CalibrationViewModel.h"
#include <limits>
#include <tobiigaze_discovery.h>
#include <tobiigaze_calibration.h>

wxPoint2DDouble CalibrationViewModel::m_calibrationPoints[] =
{
    wxPoint2DDouble(0.5, 0.5),
    wxPoint2DDouble(0.9, 0.1),
    wxPoint2DDouble(0.9, 0.9),
    wxPoint2DDouble(0.1, 0.9),
    wxPoint2DDouble(0.1, 0.1)
};

int CalibrationViewModel::m_calibrationPointCount = sizeof(m_calibrationPoints) / sizeof(wxPoint2DDouble);

CalibrationViewModel::CalibrationViewModel()
    : m_stage(INITIALIZING), m_updateNotificationCallback(NULL), m_eyeTracker(NULL)
{
    const int urlSize = 256;
    char url[urlSize];
    tobiigaze_error_code errorCode;
    tobiigaze_get_connected_eye_tracker(url, urlSize, &errorCode);
    if (errorCode)
    {
        HandleError(errorCode);
        return;
    }

    m_eyeTracker = tobiigaze_create(url, &errorCode);
    if (errorCode)
    {
        HandleError(errorCode);
        return;
    }

    tobiigaze_run_event_loop_on_internal_thread(m_eyeTracker, NULL, NULL);

    // Once the eye tracker instance has been created, and the event loop started, we can set off the call chain:
    // connect -> calibration_start -> start_tracking -> StartPositioningGuide.

    tobiigaze_connect_async(m_eyeTracker, OnConnectCompleted, this);
}

CalibrationViewModel::~CalibrationViewModel()
{
    if (m_eyeTracker)
    {
        tobiigaze_break_event_loop(m_eyeTracker);
        tobiigaze_destroy(m_eyeTracker);
        m_eyeTracker = NULL;
    }
}

void CalibrationViewModel::RegisterUpdateNotificationCallback(Callback callback, void* userData)
{
    wxMutexLocker lock(m_updateNotificationMutex);
    m_updateNotificationCallback = callback;
    m_updateNotificationUserData = userData;
}

void CalibrationViewModel::Continue()
{
    if (m_stage == POSITIONING_GUIDE)
    {
        StartCalibration();
    }
    else if (m_stage == FINISHED || m_stage == FAILED)
    {
        Exit();
    }
    // else ignore.
}

void CalibrationViewModel::Exit()
{
    m_stage = EXITING;
    RaiseUpdateNotification();
}

void CalibrationViewModel::BeginAnimationFrame()
{
    if (m_stage == CALIBRATION)
    {
        UpdateCalibrationDotAnimation();
    }
}

void CalibrationViewModel::OnConnectCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        // enter the calibration state and clear the temporary calibration buffer.
        tobiigaze_calibration_start_async(thus->m_eyeTracker, OnCalibrationStartCompleted, thus);
    }
}

void CalibrationViewModel::OnCalibrationStartCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        // start tracking, and set up the OnGazeData function to be called every time gaze data is available.
        tobiigaze_start_tracking_async(thus->m_eyeTracker, OnStartTrackingCompleted, OnGazeData, thus);
    }
}

void CalibrationViewModel::OnStartTrackingCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        thus->StartPositioningGuide();
    }
}

void CalibrationViewModel::OnGazeData(const tobiigaze_gaze_data* gazeData, const tobiigaze_gaze_data_extensions* gazeDataExtensions, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;

    // mirror the x coordinate to make the visualization make sense.
    wxPoint2DDouble left(1 - gazeData->left.eye_position_in_track_box_normalized.x, gazeData->left.eye_position_in_track_box_normalized.y);
    wxPoint2DDouble right(1 - gazeData->right.eye_position_in_track_box_normalized.x, gazeData->right.eye_position_in_track_box_normalized.y);

    // filter out invalid data samples: the eye positions are set to NaN if the tracking status is unsatisfactory.
    switch (gazeData->tracking_status)
    {
    case TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED:
        {
            double z = (gazeData->left.eye_position_in_track_box_normalized.z + gazeData->right.eye_position_in_track_box_normalized.z) / 2;
            thus->SetEyePositions(left, right, z);
        }
        break;

    case TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED:
        right.m_x = right.m_y = std::numeric_limits<double>::quiet_NaN();
        thus->SetEyePositions(left, right, gazeData->left.eye_position_in_track_box_normalized.z);
        break;

    case TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED:
        left.m_x = left.m_y = std::numeric_limits<double>::quiet_NaN();
        thus->SetEyePositions(left, right, gazeData->right.eye_position_in_track_box_normalized.z);
        break;

    default:
        right.m_x = right.m_y = std::numeric_limits<double>::quiet_NaN();
        left.m_x = left.m_y = std::numeric_limits<double>::quiet_NaN();
        thus->SetEyePositions(left, right, 1.1);
    }
}

void CalibrationViewModel::OnCalibrationAddPointCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        // a calibration point was added successfully. move on to the next if there are more to go. 
        // otherwise move on to the next stage.
        if (thus->m_currentCalibrationPoint + 1 < m_calibrationPointCount)
        {
            thus->m_currentCalibrationPoint++;
            thus->StartCalibrationDotAnimation();
        }
        else
        {
            thus->m_stage = COMPUTING_CALIBRATION;
            tobiigaze_calibration_compute_and_set_async(thus->m_eyeTracker, OnCalibrationComputeAndSetCompleted, thus);
        }
    }
}

void CalibrationViewModel::OnCalibrationComputeAndSetCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        // leave the calibration state.
        tobiigaze_calibration_stop_async(thus->m_eyeTracker, OnCalibrationStopCompleted, thus);
    }
}

void CalibrationViewModel::OnCalibrationStopCompleted(tobiigaze_error_code errorCode, void* userData)
{
    CalibrationViewModel* thus = (CalibrationViewModel*)userData;
    if (errorCode)
    {
        thus->HandleError(errorCode);
    }
    else
    {
        thus->m_stage = FINISHED;
        thus->RaiseUpdateNotification();
    }
}

void CalibrationViewModel::HandleError(tobiigaze_error_code errorCode)
{
    m_stage = FAILED;
    m_errorMessage = wxString::FromUTF8(tobiigaze_get_error_message(errorCode));
    RaiseUpdateNotification();
}

void CalibrationViewModel::SetEyePositions(wxPoint2DDouble left, wxPoint2DDouble right, double z)
{
    const double calibrationNearLimit = 0.3;
    const double calibrationFarLimit = 0.7;

    m_leftEyePosition = left;
    m_rightEyePosition = right;

    if (z < calibrationNearLimit)
    {
        m_positioningStatus = TOO_CLOSE;
    }
    else if (z <= calibrationFarLimit)
    {
        m_positioningStatus = POSITION_OK;
    }
    else
    {
        m_positioningStatus = TOO_FAR_OR_NOT_DETECTED;
    }

    RaiseUpdateNotification();
}

void CalibrationViewModel::RaiseUpdateNotification()
{
    wxMutexLocker lock(m_updateNotificationMutex);
    if (m_updateNotificationCallback)
    {
        m_updateNotificationCallback(m_updateNotificationUserData);
    }
}

void CalibrationViewModel::StartPositioningGuide()
{
    m_stage = POSITIONING_GUIDE;
    wxPoint2DDouble dummy(std::numeric_limits<double>::quiet_NaN(), std::numeric_limits<double>::quiet_NaN());
    SetEyePositions(dummy, dummy, 1.1);
}

void CalibrationViewModel::StartCalibration()
{
    m_stage = CALIBRATION;
    m_currentCalibrationPoint = 0;
    StartCalibrationDotAnimation();
}

void CalibrationViewModel::StartCalibrationDotAnimation()
{
    // clear the addPointOperationInProgress flag, (re)start the animation clock, and raise an update 
    // request to trigger the drawing of the first animation frame.
    m_addPointOperationInProgress = false;
    m_animationClock.Start();
    RaiseUpdateNotification();
}

void CalibrationViewModel::UpdateCalibrationDotAnimation()
{
    static const double PI = 3.14159265358979323846;
    static const double DURATION_MILLIS = 4000.0;

    long millis = m_animationClock.Time();
    if (millis <= DURATION_MILLIS)
    {
        // during the first DURATION_MILLIS milliseconds we run a grow/shrink animation.
        m_calibrationDotSize = sin(millis / DURATION_MILLIS * PI);

        // raise an update request to trigger the drawing of the next frame.
        RaiseUpdateNotification();
    }
    else if (!m_addPointOperationInProgress)
    {
        // when the animation has finished, we call calibration_add_point (once), which will in turn 
        // call OnCalibrationAddPointCompleted.
        m_addPointOperationInProgress = true;
        tobiigaze_point_2d point = { GetCalibrationDotPosition().m_x, GetCalibrationDotPosition().m_y };
        tobiigaze_calibration_add_point_async(m_eyeTracker, &point, OnCalibrationAddPointCompleted, this);
    }
}
