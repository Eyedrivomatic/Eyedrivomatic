/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#include "TestingViewModel.h"
#include <limits>

TestingViewModel::TestingViewModel(int testCase)
    : m_testCase(testCase), m_updateNotificationCallback(NULL)
{
    Init();
}

void TestingViewModel::RegisterUpdateNotificationCallback(Callback callback, void* userData)
{
    m_updateNotificationCallback = callback;
    m_updateNotificationUserData = userData;
}

void TestingViewModel::Continue() 
{ 
    m_testCase++;
    Init();
    RaiseUpdateNotification();
}

void TestingViewModel::Exit()
{
    m_stage = EXITING;
    RaiseUpdateNotification();
}

void TestingViewModel::BeginAnimationFrame()
{
    static const double PI = 3.14159265358979323846;

    if (m_testCase == 6)
    {
        long millis = m_animationClock.Time();
        m_calibrationDotSize = abs(sin(millis / 4000.0 * PI));
        RaiseUpdateNotification();
    }
}

void TestingViewModel::Init()
{
    switch (m_testCase)
    {
    case 0:
        m_stage = INITIALIZING;
        break;

    case 1:
        m_stage = POSITIONING_GUIDE;
        m_leftEyePosition = wxPoint2DDouble(0, 0);
        m_rightEyePosition = wxPoint2DDouble(std::numeric_limits<double>::quiet_NaN(), std::numeric_limits<double>::quiet_NaN());
        break;

    case 2:
        m_stage = POSITIONING_GUIDE;
        m_positioningStatus = TOO_CLOSE;
        m_leftEyePosition = wxPoint2DDouble(0, 0);
        m_rightEyePosition = wxPoint2DDouble(1, 1);
        break;

    case 3:
        m_stage = CALIBRATION;
        m_calibrationDotPosition = wxPoint2DDouble(0.1, 0.1);
        m_calibrationDotSize = 1.0;
        break;

    case 4:
        m_stage = CALIBRATION;
        m_calibrationDotPosition = wxPoint2DDouble(0, 0);
        m_calibrationDotSize = 1.0;
        break;

    case 5:
        m_stage = CALIBRATION;
        m_calibrationDotPosition = wxPoint2DDouble(1, 1);
        m_calibrationDotSize = 1.0;
        break;

    case 6:
        m_stage = CALIBRATION;
        m_calibrationDotPosition = wxPoint2DDouble(0.5, 0.5);
        m_calibrationDotSize = 0;
        m_animationClock.Start();
        break;

    case 7:
        m_stage = COMPUTING_CALIBRATION;
        break;

    case 8:
        m_stage = FINISHED;
        break;

    case 9:
        m_stage = FAILED;
        m_errorMessage = wxT("There has been a failure on the internets!");
        break;

    default:
        m_stage = EXITING;
        break;
    }
}

void TestingViewModel::RaiseUpdateNotification()
{
    if (m_updateNotificationCallback)
    {
        m_updateNotificationCallback(m_updateNotificationUserData);
    }
}
