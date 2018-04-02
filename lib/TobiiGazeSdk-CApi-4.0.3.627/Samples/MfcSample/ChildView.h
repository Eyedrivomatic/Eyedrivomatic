/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once
#include <list>

class CEyeTrackingEngine;


// CChildView window

class CChildView : public CWnd
{
// Construction
public:
        CChildView(CEyeTrackingEngine *eyeTrackingEngine);

// Attributes
public:

// Operations
public:

// Overrides
protected:
        virtual BOOL PreCreateWindow(CREATESTRUCT& cs);

// Implementation
public:
        virtual ~CChildView();

private:
        struct Point
        {
            double x;
            double y;
        };

        CEyeTrackingEngine *m_eyeTrackingEngine;
        SRWLOCK m_lock;
        std::list<Point> m_points;

        void AddPoint(const Point& point);
        static void WINAPI OnGazePoint(double x, double y, LPVOID lpParameter);

        // Generated message map functions
protected:
        DECLARE_MESSAGE_MAP()
        afx_msg void OnPaint();
        afx_msg LRESULT OnUpdateView(WPARAM wparam, LPARAM lparam);
};
