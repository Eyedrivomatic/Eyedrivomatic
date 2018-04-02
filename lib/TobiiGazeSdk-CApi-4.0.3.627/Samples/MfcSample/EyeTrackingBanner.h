/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include "EyeTrackingState.h"

class CEyeTrackingEngine;


class CEyeTrackingBanner : public CWnd
{
        DECLARE_DYNAMIC(CEyeTrackingBanner)

public:
        CEyeTrackingBanner(CEyeTrackingEngine *eyeTrackingEngine);
        virtual ~CEyeTrackingBanner();

        int GetRequiredWindowHeight();

private:
        EyeTrackingState m_currentState;
        CEyeTrackingEngine *m_eyeTrackingEngine;

        CButton m_hideButton;
        CButton m_retryButton;

        void SetState(EyeTrackingState state);
        void UpdateLayout(int cx, int cy);
        static bool WindowShouldBeVisible(EyeTrackingState state);
        static bool CanRetry(EyeTrackingState state);
        static void WINAPI OnStateChanged(LPVOID lpParameter);

public:
        DECLARE_MESSAGE_MAP()
        afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
        afx_msg void OnSize(UINT nType, int cx, int cy);
        afx_msg void OnPaint();
        afx_msg LRESULT OnUpdateState(WPARAM wparam, LPARAM lparam);
        afx_msg void OnHideButtonClicked();
        afx_msg void OnRetryButtonClicked();
};
