/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#include "CalibrationWindow.h"
#include <wx/graphics.h>
#include <wx/dcbuffer.h>
#include <wx/event.h>
#include <stdexcept>

// custom event type used by the CalibrationPanel to trigger a refresh.
DECLARE_EVENT_TYPE(REFRESH_EVENT, -1);

DEFINE_EVENT_TYPE(REFRESH_EVENT)

BEGIN_EVENT_TABLE(CalibrationPanel, wxPanel)
    EVT_PAINT(CalibrationPanel::OnPaint)
    EVT_COMMAND(wxID_ANY, REFRESH_EVENT, CalibrationPanel::OnRefresh)
    EVT_CHAR(CalibrationPanel::OnChar)
END_EVENT_TABLE()

CalibrationPanel::CalibrationPanel(wxWindow* parent, ICalibrationViewModel& viewModel)
: wxPanel(parent, -1, wxDefaultPosition, wxDefaultSize, wxFULL_REPAINT_ON_RESIZE, wxT("")), 
    m_viewModel(viewModel)
{
#if wxCHECK_VERSION(3, 0, 0)
    SetBackgroundStyle(wxBG_STYLE_PAINT);
#else
    SetBackgroundStyle(wxBG_STYLE_CUSTOM);
#endif

    m_viewModel.RegisterUpdateNotificationCallback(OnUpdateNotification, this);
}

CalibrationPanel::~CalibrationPanel()
{
    // unregister from the view model to ensure that no callbacks are made to the ghost of this object.
    m_viewModel.RegisterUpdateNotificationCallback(NULL, NULL);
}

void CalibrationPanel::OnPaint(wxPaintEvent& paint)
{
    wxBufferedPaintDC dc(this);
    wxGraphicsContext* context = wxGraphicsContext::Create(dc);
    if (!context) throw std::runtime_error("Could not create a wxGraphicsContext.");

    // inform the view model that rendering begins, so that it can update its animation state accordingly.
    m_viewModel.BeginAnimationFrame();

    // clear the background
    wxSize clientSize = GetClientSize();
    context->SetPen(*wxTRANSPARENT_PEN);
    context->SetBrush(*wxGREY_BRUSH);
    context->DrawRectangle(0, 0, clientSize.GetWidth(), clientSize.GetHeight());

    // different strokes for different stages.
    switch (m_viewModel.GetStage())
    {
    case ICalibrationViewModel::INITIALIZING:
        PaintMessage(wxT("Initializing, please wait..."), *context);
        break;

    case ICalibrationViewModel::FAILED:
        {
            wxString message(m_viewModel.GetErrorMessage());
            message.Append(wxT(" Press space to exit."));
            PaintMessage(message, *context);
        }
        break;

    case ICalibrationViewModel::POSITIONING_GUIDE:
        PaintPositioningGuide(wxSize(500, 300), *context);
        break;

    case ICalibrationViewModel::CALIBRATION:
        PaintCalibrationDot(*context);
        break;

    case ICalibrationViewModel::COMPUTING_CALIBRATION:
        PaintMessage(wxT("Computing calibration, please wait..."), *context);
        break;

    case ICalibrationViewModel::FINISHED:
        PaintMessage(wxT("Done! Press space to exit."), *context);
        break;

    default:
        // do nothing
        break;
    }

    delete context;
}

void CalibrationPanel::OnRefresh(wxCommandEvent& command)
{
    if (m_viewModel.GetStage() == ICalibrationViewModel::EXITING)
    {
        GetParent()->Close();
    }
    else
    {
        Refresh();
    }
}

void CalibrationPanel::OnChar(wxKeyEvent& event)
{
    switch (event.GetUnicodeKey())
    {
    case WXK_SPACE:
        m_viewModel.Continue();
        break;

    case WXK_ESCAPE:
        m_viewModel.Exit();
        break;
    }
}

void CalibrationPanel::PaintMessage(const wxString& message, wxGraphicsContext& context)
{
    wxSize clientSize = GetClientSize();

    context.SetFont(GetFont(), *wxBLACK);
    double textWidth, textHeight, descent, leading;
    context.GetTextExtent(message, &textWidth, &textHeight, &descent, &leading);
    context.DrawText(message, (clientSize.GetWidth() - textWidth) / 2, (clientSize.GetHeight() - textHeight) / 2);
}

void CalibrationPanel::PaintPositioningGuide(wxSize boxSize, wxGraphicsContext& context)
{
    wxSize clientSize = GetClientSize();

    wxPoint boxPosition((clientSize.GetWidth() - boxSize.GetWidth()) / 2, (clientSize.GetHeight() - boxSize.GetHeight()) / 2);

    context.SetPen(*wxTRANSPARENT_PEN);
    wxBrush grayBrush(wxColour(0x404040));
    context.SetBrush(grayBrush);
    context.DrawRectangle(boxPosition.x, boxPosition.y, boxSize.GetWidth(), boxSize.GetHeight());

    wxRect boxRect(boxPosition, boxSize);
    PaintEye(m_viewModel.GetLeftEyePosition(), boxRect, context);
    PaintEye(m_viewModel.GetRightEyePosition(), boxRect, context);

    wxString message;
    switch (m_viewModel.GetPositioningStatus())
    {
    case ICalibrationViewModel::TOO_CLOSE:
        message = wxT("Sit at arm's length from the screen");
        break;

    case ICalibrationViewModel::TOO_FAR_OR_NOT_DETECTED:
        message = wxT("Move closer");
        break;

    default:
        message = wxT("Press space to continue...");
        break;
    }

    context.SetFont(GetFont(), *wxBLACK);
    double textWidth, textHeight, descent, leading;
    context.GetTextExtent(message, &textWidth, &textHeight, &descent, &leading);
    context.DrawText(message, (clientSize.GetWidth() - textWidth) / 2, boxPosition.y + boxSize.GetHeight() + 10);
}

void CalibrationPanel::PaintEye(wxPoint2DDouble point, wxRect boxRect, wxGraphicsContext& context)
{
    const int eyeRadius = 10;

    if (!wxIsNaN(point.m_x))
    {
        double x = boxRect.GetLeft() + eyeRadius + point.m_x * (boxRect.GetWidth() - 4 * eyeRadius);
        double y = boxRect.GetTop() + eyeRadius + point.m_y * (boxRect.GetHeight() - 4 * eyeRadius);

        context.SetPen(*wxTRANSPARENT_PEN);
        context.SetBrush(*wxWHITE_BRUSH);
        context.DrawEllipse(x, y, 2 * eyeRadius, 2 * eyeRadius);
    }
}

void CalibrationPanel::PaintCalibrationDot(wxGraphicsContext& context)
{
    const double bigRadius = 25;
    const double smallRadius = 3;

    wxPoint2DDouble pos = m_viewModel.GetCalibrationDotPosition();
    double size = m_viewModel.GetCalibrationDotSize();
    if (!wxIsNaN(pos.m_x) && size >= 0)
    {
        wxSize clientSize = GetClientSize();
        wxPoint pixelPos(pos.m_x * clientSize.GetWidth(), pos.m_y * clientSize.GetHeight());

        context.SetPen(*wxTRANSPARENT_PEN);
        context.SetBrush(*wxWHITE_BRUSH);
        context.DrawEllipse(pixelPos.x - size * bigRadius, pixelPos.y - size * bigRadius, 2 * size * bigRadius, 2 * size * bigRadius);

        context.SetBrush(*wxBLACK_BRUSH);
        context.DrawEllipse(pixelPos.x - smallRadius, pixelPos.y - smallRadius, 2 * smallRadius, 2 * smallRadius);
    }
}

void CalibrationPanel::OnUpdateNotification(void* userData)
{
    CalibrationPanel* thus = (CalibrationPanel*)userData;

    // since this method is typically called from a thread other than the GUI thread, we'll post 
    // an event to the GUI thread and handle it there.
    wxCommandEvent event(REFRESH_EVENT);
    thus->AddPendingEvent(event);
}

CalibrationWindow::CalibrationWindow(ICalibrationViewModel& viewModel)
    : wxFrame(NULL, wxID_ANY, wxT("Eye Tracker Calibration Sample"), wxDefaultPosition, wxDefaultSize, wxBORDER_NONE | wxMAXIMIZE | wxSTAY_ON_TOP)
{
    m_panel = new CalibrationPanel(this, viewModel);
    ShowFullScreen(true, wxFULLSCREEN_ALL);
    m_panel->SetFocus();
}
