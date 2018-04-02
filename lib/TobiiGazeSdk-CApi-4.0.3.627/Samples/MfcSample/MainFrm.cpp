/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include "stdafx.h"
#include "MfcSample.h"

#include "MainFrm.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CMainFrame

IMPLEMENT_DYNAMIC(CMainFrame, CFrameWnd)

BEGIN_MESSAGE_MAP(CMainFrame, CFrameWnd)
        ON_WM_CREATE()
        ON_WM_SETFOCUS()
        ON_WM_MOVING()
END_MESSAGE_MAP()

// CMainFrame construction/destruction

CMainFrame::CMainFrame(CEyeTrackingEngine *eyeTrackingEngine)
        : m_eyeTrackingBanner(eyeTrackingEngine), m_wndView(eyeTrackingEngine)
{
}

CMainFrame::~CMainFrame()
{
}

int CMainFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
        if (CFrameWnd::OnCreate(lpCreateStruct) == -1)
                return -1;

        if (!m_eyeTrackingBanner.Create(NULL, NULL, WS_CHILD | WS_VISIBLE, CRect(0, 0, 0, 0), this, AFX_IDW_PANE_FIRST, NULL))
        {
                TRACE0("Failed to create eye tracking banner window\n");
                return -1;
        }

        // create a view to occupy the client area of the frame
        if (!m_wndView.Create(NULL, NULL, WS_CHILD | WS_VISIBLE, CRect(0, 0, 0, 0), this, AFX_IDW_PANE_FIRST + 1, NULL))
        {
                TRACE0("Failed to create view window\n");
                return -1;
        }

        return 0;
}

BOOL CMainFrame::PreCreateWindow(CREATESTRUCT& cs)
{
        if( !CFrameWnd::PreCreateWindow(cs) )
                return FALSE;
        // TODO: Modify the Window class or styles here by modifying
        //  the CREATESTRUCT cs

        cs.style |= CS_HREDRAW|CS_VREDRAW;
        cs.dwExStyle &= ~WS_EX_CLIENTEDGE;
        cs.lpszClass = AfxRegisterWndClass(0);
        return TRUE;
}

// CMainFrame diagnostics

#ifdef _DEBUG
void CMainFrame::AssertValid() const
{
        CFrameWnd::AssertValid();
}

void CMainFrame::Dump(CDumpContext& dc) const
{
        CFrameWnd::Dump(dc);
}
#endif //_DEBUG


// CMainFrame message handlers

void CMainFrame::OnSetFocus(CWnd* /*pOldWnd*/)
{
        // forward focus to the view window
        m_wndView.SetFocus();
}

BOOL CMainFrame::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
        // let the view have first crack at the command
        if (m_wndView.OnCmdMsg(nID, nCode, pExtra, pHandlerInfo))
                return TRUE;

        // otherwise, do default handling
        return CFrameWnd::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

void CMainFrame::RecalcLayout(BOOL bNotify)
{
        if (m_wndView.GetSafeHwnd() == 0)
        {
                return;
        }

        CRect bounds;
        GetClientRect(&bounds);

        int bannerHeight = m_eyeTrackingBanner.GetRequiredWindowHeight();
        m_eyeTrackingBanner.MoveWindow(bounds.left, bounds.top, bounds.right - bounds.left, bannerHeight);

        bounds.DeflateRect(0, bannerHeight, 0, 0);
        m_wndView.MoveWindow(&bounds);
}

void CMainFrame::OnMoving(UINT fwSide, LPRECT pRect)
{
        CFrameWnd::OnMoving(fwSide, pRect);

        Invalidate();
}
