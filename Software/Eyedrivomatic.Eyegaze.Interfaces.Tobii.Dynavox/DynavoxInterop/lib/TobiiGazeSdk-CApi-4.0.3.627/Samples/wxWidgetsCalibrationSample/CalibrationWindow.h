/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include <wx/wx.h>
#include "ICalibrationViewModel.h"

class wxGraphicsContext;

/**
 * Calibration panel. Draws whatever the ICalibrationViewModel tells it to. 
 * Listens to keyboard events and calls the Continue and Exit methods on the view model accordingly.
 */
class CalibrationPanel : public wxPanel
{
public:
    CalibrationPanel(wxWindow* parent, ICalibrationViewModel& viewModel);
    ~CalibrationPanel();

private:
    // Event handlers.
    void OnPaint(wxPaintEvent& paint);
    void OnRefresh(wxCommandEvent& command);
    void OnChar(wxKeyEvent& event);
    DECLARE_EVENT_TABLE();

    // Drawing operations.
    void PaintMessage(const wxString& message, wxGraphicsContext& context);
    void PaintPositioningGuide(wxSize boxSize, wxGraphicsContext& context);
    void PaintEye(wxPoint2DDouble point, wxRect boxRect, wxGraphicsContext& context);
    void PaintCalibrationDot(wxGraphicsContext& context);

    // Callback method registered with the view model. Invoked when the view should refresh its contents.
    static void OnUpdateNotification(void* userData);

    ICalibrationViewModel& m_viewModel;
};

/**
 * Calibration window. Houses the CalibrationPanel.
 * (The reason why the calibration window is implemented as a frame and panel, instead 
 * of only the frame, is that frames cannot receive keyboard events on GTK.)
 */
class CalibrationWindow : public wxFrame
{
public:
    CalibrationWindow(ICalibrationViewModel& viewModel);

private:
    CalibrationPanel* m_panel;
};
