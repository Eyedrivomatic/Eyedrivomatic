/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include "StdAfx.h"
#include "EyeTrackingEngine.h"
#include <tobiigaze.h>
#include <tobiigaze_data_types.h>
#include <tobiigaze_discovery.h>

#define MAX_EYE_TRACKER_URL_LENGTH 256

void SetThreadName( DWORD dwThreadID, char* threadName);

CEyeTrackingEngine::CEyeTrackingEngine()
        : m_state(ETS_NOT_INITIALIZED), m_eventLoopThread(0), m_eyeTracker(NULL)
{
}

CEyeTrackingEngine::~CEyeTrackingEngine()
{
        Reset();
}

void CEyeTrackingEngine::RegisterStateChangedListener(StateChangedListener listener, void *user_data)
{
        m_stateChangedListeners.push_back(std::pair<StateChangedListener, void*>(listener, user_data));
}

void CEyeTrackingEngine::UnregisterStateChangedListener(StateChangedListener listener, void *user_data)
{
        m_stateChangedListeners.remove(std::pair<StateChangedListener, void*>(listener, user_data));
}

void CEyeTrackingEngine::RegisterGazePointListener(GazePointListener listener, void *user_data)
{
        m_gazePointListeners.push_back(std::pair<GazePointListener, void*>(listener, user_data));
}

void CEyeTrackingEngine::UnregisterGazePointListener(GazePointListener listener, void *user_data)
{
        m_gazePointListeners.remove(std::pair<GazePointListener, void*>(listener, user_data));
}

void CEyeTrackingEngine::Initialize()
{
        ASSERT(GetState() == ETS_NOT_INITIALIZED);

        if (InitializeEyeTracker() &&
                StartEventLoop())
        {
                BeginConnectEyeTracker();
        }
}

EyeTrackingState CEyeTrackingEngine::GetState()
{
        return m_state;
}

void CEyeTrackingEngine::SuppressErrorMessage()
{
        SetState(ETS_ERROR_SUPPRESSED);
}

void CEyeTrackingEngine::Retry()
{
        Reset();
        Initialize();
}

void CEyeTrackingEngine::Reset()
{
        StopEventLoop();

        m_eyeTrackerUrl.clear();

        if (m_eyeTracker != NULL)
        {
                tobiigaze_destroy(m_eyeTracker);
                m_eyeTracker = NULL;
        }

        SetState(ETS_NOT_INITIALIZED);
}

void CEyeTrackingEngine::SetState(EyeTrackingState state)
{
        if (state != m_state)
        {
                m_state = state;
                RaiseStateChanged();
        }
}

void CEyeTrackingEngine::RaiseStateChanged()
{
        for (std::list<std::pair<StateChangedListener, void*>>::iterator iter = m_stateChangedListeners.begin();
                iter != m_stateChangedListeners.end();
                iter++)
        {
                iter->first(iter->second);
        }
}

bool CEyeTrackingEngine::InitializeEyeTracker()
{
        char url[MAX_EYE_TRACKER_URL_LENGTH];
        tobiigaze_error_code error_code;
        tobiigaze_get_connected_eye_tracker(url, MAX_EYE_TRACKER_URL_LENGTH, &error_code);
        if (error_code != TOBIIGAZE_ERROR_SUCCESS)
        {
                SetState(ETS_EYE_TRACKER_NOT_FOUND);
                return false;
        }
        m_eyeTrackerUrl.assign(url);

        m_eyeTracker = tobiigaze_create(url, &error_code);

        if (error_code)
        {
                SetState(ETS_CONNECTION_FAILED);
                return false;
        }

        tobiigaze_register_error_callback(m_eyeTracker, &OnStatusUpdate, this);

        return true;
}

void CEyeTrackingEngine::BeginConnectEyeTracker()
{
        SetState(ETS_CONNECTING);
        tobiigaze_connect_async(m_eyeTracker, &OnConnectFinished, this);
}

bool CEyeTrackingEngine::StartEventLoop()
{
        ASSERT(m_eventLoopThread == 0);

        DWORD threadID(0);
        m_eventLoopThread = CreateThread(NULL, 0, &EventLoop, this, 0, &threadID);

        // NOTE naming the event loop thread is by no means necessary, but it makes tracing through the code slightly easier.
        SetThreadName(threadID, "Eye Tracking Engine Event Loop");

        return m_eventLoopThread != 0;
}

void CEyeTrackingEngine::StopEventLoop()
{
        if (m_eventLoopThread != 0)
        {
                tobiigaze_break_event_loop(m_eyeTracker);
                WaitForSingleObject(m_eventLoopThread, INFINITE);
                CloseHandle(m_eventLoopThread);
                m_eventLoopThread = 0;
        }
}

void CEyeTrackingEngine::RaiseGazePoint(const tobiigaze_point_2d& point)
{
        for (std::list<std::pair<GazePointListener, void*>>::iterator iter = m_gazePointListeners.begin();
                iter != m_gazePointListeners.end();
                iter++)
        {
                iter->first(point.x, point.y, iter->second);
        }
}

void CEyeTrackingEngine::OnStatusUpdate(tobiigaze_error_code error_code, void* user_data)
{
        CEyeTrackingEngine& instance = *(CEyeTrackingEngine*)user_data;

        if (error_code != TOBIIGAZE_ERROR_SUCCESS)
        {
                instance.SetState(ETS_CONNECTION_FAILED);
                return;
        }
}

void CEyeTrackingEngine::OnConnectFinished(tobiigaze_error_code error_code, void* user_data)
{
        CEyeTrackingEngine& instance = *(CEyeTrackingEngine*)user_data;

        if (error_code != TOBIIGAZE_ERROR_SUCCESS)
        {
                instance.SetState(ETS_CONNECTION_FAILED);
                return;
        }

        instance.SetState(ETS_STARTING_TRACKING);
        tobiigaze_start_tracking_async(instance.m_eyeTracker, &OnStartTrackingFinished, &OnGazeData, &instance);
}

void CEyeTrackingEngine::OnStartTrackingFinished(tobiigaze_error_code error_code, void* user_data)
{
        CEyeTrackingEngine& instance = *(CEyeTrackingEngine*)user_data;

        if (error_code != TOBIIGAZE_ERROR_SUCCESS)
        {
                instance.SetState(ETS_CONNECTION_FAILED);
                return;
        }

        instance.SetState(ETS_TRACKING);
}

void CEyeTrackingEngine::OnGazeData(const struct tobiigaze_gaze_data* gaze_data, const tobiigaze_gaze_data_extensions* extensions, void* user_data)
{
        CEyeTrackingEngine& instance = *(CEyeTrackingEngine*)user_data;

        if (gaze_data->tracking_status == TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED)
        {
                tobiigaze_point_2d p;
                p.x = (gaze_data->left.gaze_point_on_display_normalized.x + gaze_data->right.gaze_point_on_display_normalized.x) / 2;
                p.y = (gaze_data->left.gaze_point_on_display_normalized.y + gaze_data->right.gaze_point_on_display_normalized.y) / 2;
                instance.RaiseGazePoint(p);
        }
        else if (gaze_data->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED ||
                gaze_data->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_LEFT)
        {
                instance.RaiseGazePoint(gaze_data->left.gaze_point_on_display_normalized);
        }
        else if (gaze_data->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED ||
                gaze_data->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_RIGHT)
        {
                instance.RaiseGazePoint(gaze_data->right.gaze_point_on_display_normalized);
        }
}

DWORD CEyeTrackingEngine::EventLoop(LPVOID lpParameter)
{
        CEyeTrackingEngine& instance = *(CEyeTrackingEngine*)lpParameter;

        tobiigaze_error_code error_code;
        tobiigaze_run_event_loop(instance.m_eyeTracker, &error_code);
        if (error_code != TOBIIGAZE_ERROR_SUCCESS)
        {
                instance.SetState(ETS_EYE_TRACKER_ERROR);
        }

        return 0;
}
