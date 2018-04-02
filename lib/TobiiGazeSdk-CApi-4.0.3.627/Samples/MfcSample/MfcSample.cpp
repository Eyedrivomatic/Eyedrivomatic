/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include "stdafx.h"
#include "afxwinappex.h"
#include "afxdialogex.h"
#include "MfcSample.h"
#include "MainFrm.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

BEGIN_MESSAGE_MAP(CMfcSampleApp, CWinApp)
        ON_COMMAND(ID_APP_ABOUT, &CMfcSampleApp::OnAppAbout)
END_MESSAGE_MAP()

CMfcSampleApp::CMfcSampleApp()
{
#ifdef _DEBUG
        AfxEnableMemoryTracking(FALSE);
        AfxEnableMemoryLeakDump(FALSE);
#endif
        // TODO: replace application ID string below with unique ID string; recommended
        // format for string is CompanyName.ProductName.SubProduct.VersionInformation
        SetAppID(_T("MfcSample.AppID.NoVersion"));

        // TODO: add construction code here,
        // Place all significant initialization in InitInstance
}

// The one and only CMfcSampleApp object
CMfcSampleApp theApp;

BOOL CMfcSampleApp::InitInstance()
{
        // InitCommonControlsEx() is required on Windows XP if an application
        // manifest specifies use of ComCtl32.dll version 6 or later to enable
        // visual styles.  Otherwise, any window creation will fail.
        INITCOMMONCONTROLSEX InitCtrls;
        InitCtrls.dwSize = sizeof(InitCtrls);
        // Set this to include all the common control classes you want to use
        // in your application.
        InitCtrls.dwICC = ICC_WIN95_CLASSES;
        InitCommonControlsEx(&InitCtrls);

        CWinApp::InitInstance();

        if (!AfxOleInit())
        {
                AfxMessageBox(IDP_OLE_INIT_FAILED);
                return FALSE;
        }

        AfxEnableControlContainer();
        EnableTaskbarInteraction(FALSE);

        // Change the registry key under which our settings are stored
        // TODO: You should modify this string to be something appropriate
        // such as the name of your company or organization
        SetRegistryKey(_T("Local AppWizard-Generated Applications"));

        CMainFrame* pFrame = new CMainFrame(&m_eyeTrackingEngine);
        if (!pFrame)
                return FALSE;
        m_pMainWnd = pFrame;
        pFrame->LoadFrame(IDR_MAINFRAME,
                WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE, NULL,
                NULL);
        pFrame->ShowWindow(SW_SHOW);
        pFrame->UpdateWindow();

        m_eyeTrackingEngine.Initialize();

        return TRUE;
}

int CMfcSampleApp::ExitInstance()
{
        //TODO: handle additional resources you may have added
        AfxOleTerm(FALSE);

        return CWinApp::ExitInstance();
}

// CMfcSampleApp message handlers


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialogEx
{
public:
        CAboutDlg();

// Dialog Data
        enum { IDD = IDD_ABOUTBOX };

protected:
        virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
        DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialogEx(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
        CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialogEx)
END_MESSAGE_MAP()

// App command to run the dialog
void CMfcSampleApp::OnAppAbout()
{
        CAboutDlg aboutDlg;
        aboutDlg.DoModal();
}

// CMfcSampleApp message handlers



