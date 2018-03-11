/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include <stdint.h>
#include <list>
#include <string>
#include <Windows.h>
#include "EyeTrackingState.h"
#include "tobiigaze_define.h"
#include "tobiigaze_data_types.h"
#include "tobiigaze_error_codes.h"

struct tobiigaze_eye_tracker;
struct tobiigaze_gaze_data;
struct tobiigaze_rect;
struct tobiigaze_point_2d;

class CEyeTrackingEngine
{
public:
        typedef void (WINAPI *StateChangedListener)(LPVOID lpParameter);
        typedef void (WINAPI *GazePointListener)(double x, double y, LPVOID lpParameter);

        CEyeTrackingEngine();
        ~CEyeTrackingEngine();

        void Initialize();
        EyeTrackingState GetState();
        void SuppressErrorMessage();
        void Retry();
        void RegisterStateChangedListener(StateChangedListener listener, void *user_data);
        void UnregisterStateChangedListener(StateChangedListener listener, void *user_data);
        void RegisterGazePointListener(GazePointListener listener, void *user_data);
        void UnregisterGazePointListener(GazePointListener listener, void *user_data);

private:
        EyeTrackingState m_state;
        std::list<std::pair<StateChangedListener, void*>> m_stateChangedListeners;
        std::list<std::pair<GazePointListener, void*>> m_gazePointListeners;
        HANDLE m_eventLoopThread;
        std::string m_eyeTrackerUrl;
        tobiigaze_eye_tracker* m_eyeTracker;

        void Reset();
        void SetState(EyeTrackingState state);
        void RaiseStateChanged();
        bool InitializeEyeTracker();
        void BeginConnectEyeTracker();
        bool StartEventLoop();
        void StopEventLoop();
        void RaiseGazePoint(const tobiigaze_point_2d& point);
        static void TOBIIGAZE_CALL OnStatusUpdate(tobiigaze_error_code error_code, void* user_data);
        static void TOBIIGAZE_CALL OnConnectFinished(tobiigaze_error_code error_code, void* user_data);
        static void TOBIIGAZE_CALL OnStartTrackingFinished(tobiigaze_error_code error_code, void* user_data);
        static void TOBIIGAZE_CALL OnGazeData(const struct tobiigaze_gaze_data* gaze_data, const tobiigaze_gaze_data_extensions* extensions, void* user_data);
        static DWORD WINAPI EventLoop(LPVOID lpParameter);
};
