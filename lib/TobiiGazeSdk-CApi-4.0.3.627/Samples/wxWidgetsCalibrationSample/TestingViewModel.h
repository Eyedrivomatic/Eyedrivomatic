/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include "ICalibrationViewModel.h"
#include <wx/stopwatch.h>

/**
 * Dummy view model for testing the calibration window. Implements the ICalibrationViewModel interface.
 * To use: change the choice of view model in CalibrationSample.cpp and start the program. You will 
 * now be able to step though a sequence of "test cases" by pressing space.
 */
class TestingViewModel : public ICalibrationViewModel
{
public:
    TestingViewModel(int testCase = 0);

    void RegisterUpdateNotificationCallback(Callback callback, void* userData);

    Stage GetStage() const { return m_stage; }
    void Continue();
    void Exit();

    wxPoint2DDouble GetLeftEyePosition() const { return m_leftEyePosition; }
    wxPoint2DDouble GetRightEyePosition() const { return m_rightEyePosition; }
    PositioningStatus GetPositioningStatus() const { return m_positioningStatus; }

    wxPoint2DDouble GetCalibrationDotPosition() const { return m_calibrationDotPosition; }
    double GetCalibrationDotSize() const { return m_calibrationDotSize; }
    void BeginAnimationFrame();

    const wxString& GetErrorMessage() const { return m_errorMessage; }

private:
    void Init();
    void RaiseUpdateNotification();

    int m_testCase;
    Stage m_stage;
    wxPoint2DDouble m_leftEyePosition;
    wxPoint2DDouble m_rightEyePosition;
    PositioningStatus m_positioningStatus;
    wxPoint2DDouble m_calibrationDotPosition;
    double m_calibrationDotSize;
    wxString m_errorMessage;
    Callback m_updateNotificationCallback;
    void* m_updateNotificationUserData;
    wxStopWatch m_animationClock;
};
