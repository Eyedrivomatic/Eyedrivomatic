/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#include <wx/wx.h>
#include "CalibrationViewModel.h"
#include "TestingViewModel.h"
#include "CalibrationWindow.h"

/**
 * Application class. Provides the application entry point.
 */
class CalibrationSampleApp : public wxApp
{
public:
    bool OnInit();
    int OnExit();

private:
    ICalibrationViewModel* m_viewModel;
};

IMPLEMENT_APP(CalibrationSampleApp);

bool CalibrationSampleApp::OnInit()
{
    // Create the view model.
    // We have the choice of a CalibrationViewModel, which does actual work, or 
    // a TestingViewModel, which can be used to test that the GUI does what it 
    // is supposed to.
    m_viewModel = new CalibrationViewModel();
    //m_viewModel = new TestingViewModel();

    CalibrationWindow *frame = new CalibrationWindow(*m_viewModel);
    frame->Show(true);

    return true;
}

int CalibrationSampleApp::OnExit()
{
    delete m_viewModel;

    return wxApp::OnExit();
}
