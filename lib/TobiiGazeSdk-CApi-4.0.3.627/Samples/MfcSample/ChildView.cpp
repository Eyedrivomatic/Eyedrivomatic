/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include "stdafx.h"
#include <Windows.h>
#include "ChildView.h"
#include "EyeTrackingEngine.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define UWM_UPDATE_VIEW WM_USER
static const int MAX_POINTS = 60;


#define __STR2__(x) #x
#define __STR1__(x) __STR2__(x)

CChildView::CChildView(CEyeTrackingEngine *eyeTrackingEngine)
        : m_eyeTrackingEngine(eyeTrackingEngine)
{
        InitializeSRWLock(&m_lock);
        m_eyeTrackingEngine->RegisterGazePointListener(&OnGazePoint, this);
}

CChildView::~CChildView()
{
        m_eyeTrackingEngine->UnregisterGazePointListener(&OnGazePoint, this);
}

void CChildView::AddPoint(const Point& point)
{
        AcquireSRWLockExclusive(&m_lock);

        m_points.push_back(point);

        while (m_points.size() > MAX_POINTS)
        {
                m_points.pop_front();
        }

        ReleaseSRWLockExclusive(&m_lock);
}

LRESULT CChildView::OnUpdateView(WPARAM wparam, LPARAM lparam)
{
        Invalidate();
        return 0;
}

void CChildView::OnGazePoint(double x, double y, LPVOID lpParameter)
{
        // NOTE this method is invoked on a background thread, but the status update affects the UI.
        // Therefore we marshal the call to the UI thread by means of a PostMessage.
        // (If the window is destroyed while the message is queued, then its message
        // pump will stop and the message won't be processed.)
        CChildView& instance = *(CChildView*)lpParameter;
        if (instance.GetSafeHwnd() != 0)
        {
                Point p = { x, y };
                instance.AddPoint(p);
                instance.PostMessage(UWM_UPDATE_VIEW);
        }
}

BEGIN_MESSAGE_MAP(CChildView, CWnd)
        ON_WM_PAINT()
        ON_MESSAGE(UWM_UPDATE_VIEW, OnUpdateView)
END_MESSAGE_MAP()

// CChildView message handlers

BOOL CChildView::PreCreateWindow(CREATESTRUCT& cs)
{
        if (!CWnd::PreCreateWindow(cs))
                return FALSE;

        cs.style &= ~WS_BORDER;
        cs.lpszClass = AfxRegisterWndClass(
                CS_HREDRAW|CS_VREDRAW|CS_DBLCLKS,
                ::LoadCursor(NULL, IDC_ARROW),
                0,
                NULL);

        return TRUE;
}

void CChildView::OnPaint()
{
        CPaintDC dc(this);

        // look up the bounds of the monitor where the window is displayed.
        // we're assuming that this is the monitor where the eye tracker is mounted.

        HMONITOR monitor = MonitorFromWindow(m_hWnd, MONITOR_DEFAULTTOPRIMARY);
        MONITORINFO monitorInfo;
        ZeroMemory(&monitorInfo, sizeof(MONITORINFO));
        monitorInfo.cbSize = sizeof(MONITORINFO);
        GetMonitorInfo(monitor, &monitorInfo);
        CRect screenBounds(monitorInfo.rcMonitor);

        // double buffered painting: the view is drawn to a bitmap which is then bitblt'ed to the screen.

        CRect clientRect;
        GetClientRect(&clientRect);

        CDC bitmapDC;
        bitmapDC.CreateCompatibleDC(&dc);

        CBitmap bitmap;
        bitmap.CreateCompatibleBitmap(&dc, clientRect.Width(), clientRect.Height());
        bitmapDC.SelectObject(&bitmap);

        CBrush backgroundBrush;
        backgroundBrush.CreateSolidBrush(RGB(0xf8, 0xf8, 0xf8));
        bitmapDC.FillRect(&clientRect, &backgroundBrush);

        AcquireSRWLockExclusive(&m_lock);

        CBrush pointBrush;
        pointBrush.CreateSolidBrush(RGB(0, 0, 0));
        bitmapDC.SelectObject(&pointBrush);
        for (std::list<Point>::iterator iter = m_points.begin(); iter != m_points.end(); iter++)
        {
                CPoint p((int)(screenBounds.left + iter->x * screenBounds.Width()), (int)(screenBounds.top + iter->y * screenBounds.Height()));
                ScreenToClient(&p);
                p.Offset(-2, -2);
                bitmapDC.Ellipse(p.x, p.y, p.x + 5, p.y + 5);
        }

        ReleaseSRWLockExclusive(&m_lock);

        dc.BitBlt(0, 0, clientRect.Width(), clientRect.Height(), &bitmapDC, 0, 0, SRCCOPY);
}
