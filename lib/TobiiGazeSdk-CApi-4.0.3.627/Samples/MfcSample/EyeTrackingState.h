/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#pragma once

enum EyeTrackingState
{
    ETS_NOT_INITIALIZED,
    ETS_EYE_TRACKER_NOT_FOUND,
    ETS_CONNECTING,
    ETS_STARTING_TRACKING,
    ETS_CONNECTION_FAILED,
    ETS_EYE_TRACKER_ERROR,
    ETS_TRACKING,
    ETS_ERROR_SUPPRESSED,
};
