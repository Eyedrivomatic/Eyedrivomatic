/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include "stdafx.h"
#include "EyeTrackingBanner.h"
#include "EyeTrackingEngine.h"
#include "Resource.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define UWM_UPDATE_STATUS WM_USER

IMPLEMENT_DYNAMIC(CEyeTrackingBanner, CWnd)

CEyeTrackingBanner::CEyeTrackingBanner(CEyeTrackingEngine *eyeTrackingEngine)
        : m_currentState(ETS_NOT_INITIALIZED), m_eyeTrackingEngine(eyeTrackingEngine)
{
        ASSERT(eyeTrackingEngine != NULL);

        m_eyeTrackingEngine->RegisterStateChangedListener(&OnStateChanged, this);
        SetState(m_eyeTrackingEngine->GetState());
}

CEyeTrackingBanner::~CEyeTrackingBanner()
{
        m_eyeTrackingEngine->UnregisterStateChangedListener(&OnStateChanged, this);
}

int CEyeTrackingBanner::GetRequiredWindowHeight()
{
        return WindowShouldBeVisible(m_currentState) ? 60 : 0;
}

void CEyeTrackingBanner::SetState(EyeTrackingState state)
{
        EyeTrackingState oldState(m_currentState);
        m_currentState = state;

        if (GetSafeHwnd() != 0)
        {
                m_retryButton.ShowWindow(CanRetry(m_currentState) ? SW_SHOW : SW_HIDE);

                if (WindowShouldBeVisible(m_currentState) != WindowShouldBeVisible(oldState))
                {
                        GetParentFrame()->RecalcLayout();
                }
                else if (m_currentState != oldState)
                {
                        CRect rect;
                        GetClientRect(&rect);
                        UpdateLayout(rect.Width(), rect.Height());

                        Invalidate();
                }
        }
}

void CEyeTrackingBanner::UpdateLayout(int cx, int cy)
{
        if (cx > 0 && cy > 0)
        {
                SIZE size;
                if (m_hideButton.GetIdealSize(&size))
                {
                        m_hideButton.MoveWindow(cx - size.cx - 5, 5, size.cx, size.cy);
                }

                int x = 5;

                if (m_retryButton.IsWindowVisible() && m_retryButton.GetIdealSize(&size))
                {
                        m_retryButton.MoveWindow(x, cy - size.cy - 5, size.cx, size.cy);
                }
        }
}

bool CEyeTrackingBanner::WindowShouldBeVisible(EyeTrackingState state)
{
        return state != ETS_NOT_INITIALIZED &&
                state != ETS_CONNECTING &&
                state != ETS_STARTING_TRACKING &&
                state != ETS_TRACKING &&
                state != ETS_ERROR_SUPPRESSED;
}

bool CEyeTrackingBanner::CanRetry(EyeTrackingState state)
{
        return state == ETS_EYE_TRACKER_NOT_FOUND ||
                state == ETS_CONNECTION_FAILED ||
                state == ETS_EYE_TRACKER_ERROR;
}

void CEyeTrackingBanner::OnStateChanged(LPVOID lpParameter)
{
        // NOTE this method is invoked on a background thread, but the status update affects the UI.
        // Therefore we marshal the call to the UI thread by means of a PostMessage.
        // (If the window is destroyed while the message is queued, then its message
        // pump will stop and the message won't be processed.)
        CEyeTrackingBanner& instance = *(CEyeTrackingBanner*)lpParameter;
        if (instance.GetSafeHwnd() != 0)
        {
                instance.PostMessage(UWM_UPDATE_STATUS);
        }
}

BEGIN_MESSAGE_MAP(CEyeTrackingBanner, CWnd)
        ON_WM_CREATE()
        ON_WM_SIZE()
        ON_WM_PAINT()
        ON_MESSAGE(UWM_UPDATE_STATUS, OnUpdateState)
        ON_BN_CLICKED(IDC_EYE_TRACKING_HIDE, OnHideButtonClicked)
        ON_BN_CLICKED(IDC_EYE_TRACKING_RETRY, OnRetryButtonClicked)
END_MESSAGE_MAP()

int CEyeTrackingBanner::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
        if (__super::OnCreate(lpCreateStruct) == -1)
                return -1;

        if (!m_hideButton.Create(_T("X"), WS_CHILD | WS_VISIBLE, CRect(0, 0, 0, 0), this, IDC_EYE_TRACKING_HIDE) ||
                !m_retryButton.Create(_T("Retry"), WS_CHILD, CRect(0, 0, 0, 0), this, IDC_EYE_TRACKING_RETRY))
        {
                return -1;
        }

        return 0;
}

void CEyeTrackingBanner::OnSize(UINT nType, int cx, int cy)
{
        __super::OnSize(nType, cx, cy);

        UpdateLayout(cx, cy);
}

void CEyeTrackingBanner::OnPaint()
{
        CPaintDC dc(this);

        CBrush brush;
        brush.CreateSysColorBrush(COLOR_INFOBK);

        CRect bounds;
        GetClientRect(&bounds);
        dc.FillRect(&bounds, &brush);

        TCHAR* message = NULL;
        switch (m_currentState)
        {
        case ETS_EYE_TRACKER_NOT_FOUND:
                message = _T("No eye tracker could be found.");
                break;

        case ETS_CONNECTION_FAILED:
                message = _T("The connection to the eye tracker failed.");
                break;

        case ETS_EYE_TRACKER_ERROR:
                message = _T("The eye tracker reported an error.");
                break;

        case ETS_NOT_INITIALIZED:
        case ETS_CONNECTING:
        case ETS_STARTING_TRACKING:
        case ETS_TRACKING:
        case ETS_ERROR_SUPPRESSED:
                message = _T("");
                break;

        default:
                ASSERT(false); // unknown state: shouldn't happen
        }

        bounds.DeflateRect(CSize(5, 5));
        dc.SetBkMode(TRANSPARENT);
        dc.DrawText(message, -1, &bounds, DT_LEFT);
}

LRESULT CEyeTrackingBanner::OnUpdateState(WPARAM wparam, LPARAM lparam)
{
        SetState(m_eyeTrackingEngine->GetState());
        return 0;
}

void CEyeTrackingBanner::OnHideButtonClicked()
{
        m_eyeTrackingEngine->SuppressErrorMessage();
}

void CEyeTrackingBanner::OnRetryButtonClicked()
{
        m_eyeTrackingEngine->Retry();
}
