/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include <stdio.h>
#include <stdlib.h>
#include "Common.h"
#include "tobiigaze_discovery.h"

/*
 * This is a simple example that demonstrates the asynchronous TobiiGazeCore calls.
 * It prints gaze data for 20 seconds.
 */

static tobiigaze_eye_tracker* eye_tracker;
static xcondition_variable disconnected_cv;

// Prints gaze information, or "-" if gaze position could not be determined.
void on_gaze_data(const tobiigaze_gaze_data* gazedata, const tobiigaze_gaze_data_extensions* extensions, void *user_data)
{
    printf("%20.3f ", gazedata->timestamp/ 1e6); // in seconds
    printf("%d ", gazedata->tracking_status);

    if (gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED ||
                gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_LEFT)
    {
        printf("[%7.4f,%7.4f] ", gazedata->left.gaze_point_on_display_normalized.x, gazedata->left.gaze_point_on_display_normalized.y);
    }
    else
    {
        printf("[%7s,%7s] ", "-", "-");
    }

    if (gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED ||
                gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_RIGHT)
    {
        printf("[%7.4f,%7.4f] ", gazedata->right.gaze_point_on_display_normalized.x, gazedata->right.gaze_point_on_display_normalized.y);
    }
    else
    {
        printf("[%7s,%7s] ", "-", "-");
    }

    printf("\n");
}

// Runs the event loop (blocking).
xthread_retval event_loop_thread_proc(void*)
{
    tobiigaze_error_code error_code;

    tobiigaze_run_event_loop(eye_tracker, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_run_event_loop");

    THREADFUNC_RETURN(error_code);
}

void on_disconnect_callback(void *user_data)
{
    printf("Disconnected.\n");

    // The tracker has been disconnected. Signal to the main thread that it is time to exit.
    xsignal_ready(&disconnected_cv);
}

void get_deviceinfo_callback(const tobiigaze_device_info* device_info, tobiigaze_error_code error_code, void *user_data)
{
    report_and_exit_on_error(error_code, "get_deviceinfo_callback");
    printf("Serial number: %s\n", device_info->serial_number);
}

void stop_tracking_callback(tobiigaze_error_code error_code, void *user_data)
{
    report_and_exit_on_error(error_code, "stop_tracking_callback");
    printf("Tracking stopped.\n");

    // Tracking has been stopped. Now disconnect.
    tobiigaze_disconnect_async(eye_tracker, &on_disconnect_callback, 0);
}

void start_tracking_callback(tobiigaze_error_code error_code, void *user_data)
{
    report_and_exit_on_error(error_code, "start_tracking_callback");
    printf("Tracking started.\n");
}

void on_connect_callback(tobiigaze_error_code error_code, void *user_data)
{
    report_and_exit_on_error(error_code, "connect_callback");
    printf("Connected.\n");

    // Now that a connection is established, retreive some device information
    // and start tracking.
    tobiigaze_get_device_info_async(eye_tracker, &get_deviceinfo_callback, 0);
    tobiigaze_start_tracking_async(eye_tracker, &start_tracking_callback, &on_gaze_data, 0);
}

void error_callback(tobiigaze_error_code error_code, void *user_data)
{
    report_and_exit_on_error(error_code, "error_callback");
}

int main(int argc, char** argv)
{
    const int urlSize = 256;
    char url[urlSize];
    tobiigaze_error_code error_code;

    xinitialize_cv(&disconnected_cv);

    // Process command-line arguments.
    if (argc == 2)
    {
        strncpy(url, argv[1], urlSize);

        if (!strcmp(url, "--auto"))
        {
            tobiigaze_get_connected_eye_tracker(url, urlSize, &error_code);
            if (error_code)
            {
                printf("No eye tracker found.\n");
                exit(-1);
            }
        }
    }
    else
    {
        printf("usage: MinimalTrackerAsync {url|--auto}\n");
        printf("example: MinimalTrackerAsync tet-tcp://172.28.195.1\n");
        return 0;
    }

    printf("TobiiGazeCore DLL version: %s\n", tobiigaze_get_version());

    // Create an eye tracker instance.
    printf("Creating eye tracker with url %s.\n", url);
    xthread_handle hThread;
    eye_tracker = tobiigaze_create(url, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_create");
    tobiigaze_register_error_callback(eye_tracker, &error_callback, 0);

    // Start the event loop. This must be done before connecting.
    hThread = xthread_create(event_loop_thread_proc, eye_tracker);

    // Connect to the tracker. The callback function is invoked when the
    // operation finishes, successfully or unsuccessfully.
    tobiigaze_connect_async(eye_tracker, &on_connect_callback, 0);

    // Let eye tracker track for 20 s, then stop tracking and disconnect.
    XSLEEP(20000);
    tobiigaze_stop_tracking_async(eye_tracker, &stop_tracking_callback, 0);

    // Wait until the tracker has been disconnected.
    if (!xwait_until_ready(&disconnected_cv))
    {
        printf("Operation timed out.\n");
        exit(-1);
    }

    // Break the event loop and join the event loop thread.
    tobiigaze_break_event_loop(eye_tracker);
    xthread_join(hThread);

    // Clean up.
    tobiigaze_destroy(eye_tracker);

    return 0;
}
