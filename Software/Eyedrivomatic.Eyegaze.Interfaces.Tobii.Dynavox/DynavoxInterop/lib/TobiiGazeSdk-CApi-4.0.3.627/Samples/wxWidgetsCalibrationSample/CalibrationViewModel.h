/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include "ICalibrationViewModel.h"
#include <wx/stopwatch.h>
#include <wx/thread.h>
#include <tobiigaze.h>

/**
 * View model for the calibration window. Implements the ICalibrationViewModel interface.
 * The view model forms the link between the calibration window and the eye tracker.
 */
class CalibrationViewModel : public ICalibrationViewModel
{
public:
    CalibrationViewModel();
    ~CalibrationViewModel();

    //
    // Implementation of the ICalibrationViewModel interface.
    //

    void RegisterUpdateNotificationCallback(Callback callback, void* userData);

    Stage GetStage() const { return m_stage; }
    void Continue();
    void Exit();

    wxPoint2DDouble GetLeftEyePosition() const { return m_leftEyePosition; }
    wxPoint2DDouble GetRightEyePosition() const { return m_rightEyePosition; }
    PositioningStatus GetPositioningStatus() const { return m_positioningStatus; }

    wxPoint2DDouble GetCalibrationDotPosition() const { return m_calibrationPoints[m_currentCalibrationPoint % m_calibrationPointCount]; }
    double GetCalibrationDotSize() const { return m_calibrationDotSize; }
    void BeginAnimationFrame();

    const wxString& GetErrorMessage() const { return m_errorMessage; }

private:
    // Callback functions used with the Tobii Gaze API.
    static void TOBIIGAZE_CALL OnConnectCompleted(tobiigaze_error_code errorCode, void* userData);
    static void TOBIIGAZE_CALL OnCalibrationStartCompleted(tobiigaze_error_code errorCode, void* userData);
    static void TOBIIGAZE_CALL OnStartTrackingCompleted(tobiigaze_error_code errorCode, void* userData);
    static void TOBIIGAZE_CALL OnGazeData(const tobiigaze_gaze_data* gazeData, const tobiigaze_gaze_data_extensions* gazeDataExtensions, void* userData);
    static void TOBIIGAZE_CALL OnCalibrationAddPointCompleted(tobiigaze_error_code errorCode, void* userData);
    static void TOBIIGAZE_CALL OnCalibrationComputeAndSetCompleted(tobiigaze_error_code errorCode, void* userData);
    static void TOBIIGAZE_CALL OnCalibrationStopCompleted(tobiigaze_error_code errorCode, void* userData);

    void HandleError(tobiigaze_error_code errorCode);
    void SetEyePositions(wxPoint2DDouble left, wxPoint2DDouble right, double z);
    void RaiseUpdateNotification();
    void StartPositioningGuide();
    void StartCalibration();
    void StartCalibrationDotAnimation();
    void UpdateCalibrationDotAnimation();

    Stage m_stage;
    wxString m_errorMessage;
    wxMutex m_updateNotificationMutex;
    Callback m_updateNotificationCallback;
    void* m_updateNotificationUserData;
    wxPoint2DDouble m_leftEyePosition;
    wxPoint2DDouble m_rightEyePosition;
    PositioningStatus m_positioningStatus;
    double m_calibrationDotSize;
    int m_currentCalibrationPoint;
    bool m_addPointOperationInProgress;
    wxStopWatch m_animationClock;

    // Eye tracker instance.
    tobiigaze_eye_tracker* m_eyeTracker;

    // Calibration points specified in the ADCS coordinate system.
    static int m_calibrationPointCount;
    static wxPoint2DDouble m_calibrationPoints[];
};
