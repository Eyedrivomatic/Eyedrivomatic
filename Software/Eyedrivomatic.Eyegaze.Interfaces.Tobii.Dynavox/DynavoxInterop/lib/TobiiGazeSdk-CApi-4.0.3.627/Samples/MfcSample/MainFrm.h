/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include "EyeTrackingBanner.h"
#include "ChildView.h"

class CMainFrame : public CFrameWnd
{

public:
        CMainFrame(CEyeTrackingEngine *eyeTrackingEngine);

protected:
        DECLARE_DYNAMIC(CMainFrame)

// Attributes
public:

// Operations
public:

// Overrides
public:
        virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
        virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

// Implementation
public:
        virtual ~CMainFrame();
#ifdef _DEBUG
        virtual void AssertValid() const;
        virtual void Dump(CDumpContext& dc) const;
#endif

private:
        CEyeTrackingBanner m_eyeTrackingBanner;
        CChildView m_wndView;

// Generated message map functions
protected:
        afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
        afx_msg void OnSetFocus(CWnd *pOldWnd);
        DECLARE_MESSAGE_MAP()

public:
        virtual void RecalcLayout(BOOL bNotify = TRUE);
        afx_msg void OnMoving(UINT fwSide, LPRECT pRect);
};
