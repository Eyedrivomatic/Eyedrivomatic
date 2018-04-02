/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once

#ifndef __AFXWIN_H__
        #error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"       // main symbols
#include "EyeTrackingEngine.h"


// CMfcSampleApp:
// See MfcSample.cpp for the implementation of this class
//

class CMfcSampleApp : public CWinApp
{
public:
        CMfcSampleApp();


// Overrides
public:
        virtual BOOL InitInstance();
        virtual int ExitInstance();

// Implementation

public:
        afx_msg void OnAppAbout();
        DECLARE_MESSAGE_MAP()

private:
        CEyeTrackingEngine m_eyeTrackingEngine;
};

extern CMfcSampleApp theApp;
