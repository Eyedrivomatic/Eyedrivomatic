/*
 * Copyright 2014 Tobii Technology AB. All rights reserved.
 */

#pragma once

#include <wx/geometry.h>

/**
 * View model interface for the calibration window.
 * The view model decides what should be displayed in the calibration window 
 * and acts on input from the calibration window.
 */
class ICalibrationViewModel
{
public:
    virtual ~ICalibrationViewModel() {}

    //
    // Update notifications: a mechanism that lets the view model inform the 
    // window that it needs to be redrawn. Note that the callback may be 
    // invoked on a thread other than the main (GUI) thread.
    //

    typedef void (*Callback)(void* userData);
    virtual void RegisterUpdateNotificationCallback(Callback callback, void* userData) = 0;

    //
    // Stages: these are the major modes that the view may be in. For some of 
    // the stages the view will need additional information to render properly.
    //

    enum Stage
    {
        INITIALIZING,
        POSITIONING_GUIDE,
        CALIBRATION,
        COMPUTING_CALIBRATION,
        FINISHED,
        EXITING,
        FAILED
    };

    virtual Stage GetStage() const = 0;

    // Move on to the next stage if possible. 
    // (Typically invoked when the user presses space.)
    virtual void Continue() = 0;

    // Go directly to the EXITING stage. Do not pass go, do not collect $200. 
    // (Typically invoked when the user presses escape.)
    virtual void Exit() = 0;

    //
    // "Positioning guide" stage specifics.
    // In this stage the visualization consists of two eye markers and an 
    // instruction to the user represented by the PositioningStatus.
    // If the eye tracker fails to detect an eye, the corresponding eye position
    // is set to NaN.
    //

    enum PositioningStatus
    {
        TOO_CLOSE,
        TOO_FAR_OR_NOT_DETECTED,
        POSITION_OK
    };

    virtual wxPoint2DDouble GetLeftEyePosition() const = 0;
    virtual wxPoint2DDouble GetRightEyePosition() const = 0;
    virtual PositioningStatus GetPositioningStatus() const = 0;

    //
    // "Calibration" stage specifics.
    // In this stage the visualization consists of a dot which can move around 
    // and change size. The view is expected to call BeginAnimationFrame right 
    // before rendering to give the view model a chance to update its state.
    //

    virtual wxPoint2DDouble GetCalibrationDotPosition() const = 0;
    virtual double GetCalibrationDotSize() const = 0;
    virtual void BeginAnimationFrame() = 0;

    //
    // "Failed" stage specifics.
    //

    virtual const wxString& GetErrorMessage() const = 0;

protected:
    ICalibrationViewModel() {}

private:
    // Copy constructor and operator declared but not implemented, making the 
    // class effectively noncopyable.
    ICalibrationViewModel(const ICalibrationViewModel&);
    ICalibrationViewModel& operator =(const ICalibrationViewModel&);
};
