/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include <stdio.h>
#include <stdlib.h>
#include "Common.h"
#include "tobiigaze_calibration.h"
#include "tobiigaze_discovery.h"

/*
 * This is an example demonstrating the mostly asynchronous calibration procedure.
 */

static tobiigaze_point_2d _calibration_points[5] = { {0.1, 0.1}, {0.9, 0.1}, { 0.9, 0.9 }, {0.1, 0.9}, {0.5, 0.5} };
static int _next_calibration_point = 0;
static int _calibration_failed = 0;
static int _calibration_done  =  0;

// forward declarations
void add_calibration_point_handler(tobiigaze_error_code error_code, void *user_data);
void compute_calibration_handler(tobiigaze_error_code error_code, void *user_data);
void stop_calibration_handler(tobiigaze_error_code error_code, void *user_data);

void handle_calibration_error(tobiigaze_error_code error_code, void *user_data, const char *error_message)
{
    if (error_code)
    {
        fprintf(stderr, "Error: %08X (%s).\n", error_code, error_message);
        _calibration_failed = 1;
        tobiigaze_calibration_stop_async((tobiigaze_eye_tracker*) user_data, stop_calibration_handler, user_data);
    }
}

void add_calibration_point_handler(tobiigaze_error_code error_code, void *user_data)
{
    if (error_code)
    {
        handle_calibration_error(error_code, user_data, "add_calibration_point_handler");
        return;
    }

    if ((size_t)_next_calibration_point < sizeof(_calibration_points) / sizeof(tobiigaze_point_2d))
    {
        // Note that most GUI frameworks do not allow background threads to access any GUI components.
        // Normally you will have to pass control to the GUI thread and let it do its job before continuing.

		//
		// TODO: Insert code here for displaying an object on the screen corresponding to the coordinates of _calibration_points[_next_calibration_point].
		//

		printf("-------------ADD POINT %d (%.1f, %.1f) ------------\n", _next_calibration_point, _calibration_points[_next_calibration_point].x, _calibration_points[_next_calibration_point].y);
		XSLEEP(2000);  // Give the user some time to move the gaze and focus on the object

		// The call to tobiigaze_calibration_add_point_async starts collecting calibration data at the specified point.
		// Make sure to keep the stimulus (i.e., the calibration dot) on the screen until the tracker is finished, that
		// is, until the callback function is invoked.
        tobiigaze_calibration_add_point_async((tobiigaze_eye_tracker*) user_data, &_calibration_points[_next_calibration_point++], add_calibration_point_handler, user_data);
    }
    else
    {
        printf("Computing calibration...\n");
        tobiigaze_calibration_compute_and_set_async((tobiigaze_eye_tracker*) user_data, compute_calibration_handler, user_data);
    }
}

void compute_calibration_handler(tobiigaze_error_code error_code, void *user_data)
{
    if (error_code)
    {
        if (error_code == TOBIIGAZE_FW_ERROR_OPERATION_FAILED)
        {
            // TODO: Replace error handling below with code for handling recalibration.
            fprintf(stderr, "Compute calibration FAILED due to insufficient gaze data.\n");
        }

        handle_calibration_error(error_code, user_data, "compute_calibration_handler");
        return;
    }

    printf("compute_calibration_handler: OK\n");
    tobiigaze_calibration_stop_async((tobiigaze_eye_tracker*) user_data, stop_calibration_handler, user_data);
}

void stop_calibration_handler(tobiigaze_error_code error_code, void *user_data)
{
    _calibration_done = 1;

    if (error_code)
    {
        handle_calibration_error(error_code, user_data, "stop_calibration_handler");
        return;
    }

    printf("stop_calibration_handler: OK\n");
}

xthread_retval _thread(void* eye_tracker)
{
    tobiigaze_error_code error_code;

    tobiigaze_run_event_loop((tobiigaze_eye_tracker*)eye_tracker, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_run_event_loop");

    THREADFUNC_RETURN(error_code)
}

void print_device_info(tobiigaze_eye_tracker* eye_tracker)
{
    tobiigaze_error_code error_code;
    tobiigaze_device_info info;

    tobiigaze_get_device_info(eye_tracker, &info, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_get_device_info");

    printf("Serial number: %s\n", info.serial_number);
}

void on_disconnect_callback(void *user_data)
{
    printf("Disconnected\n");
}

int main(int argc, char** argv)
{
	const int urlSize = 256;
    char url[urlSize];
    tobiigaze_error_code error_code;

    if (argc == 2)
    {
        strncpy(url, argv[1], urlSize);

		if (!strcmp(url, "--auto"))
		{
			tobiigaze_get_connected_eye_tracker(url, urlSize, &error_code);
			if (error_code)
			{
				printf("No eye tracker found\n");
				exit(-1);
			}
		}
    }
    else
    {
		printf("usage: MinimalCalibration {url|--auto}\n");
        printf("example: MinimalCalibration tet-tcp://172.68.195.1\n");
        return 0;
    }

    printf("TobiiGazeCore DLL version: %s\n", tobiigaze_get_version());

    printf("Creating eye tracker with url %s.\n", url);

    tobiigaze_eye_tracker* eye_tracker;
    eye_tracker = tobiigaze_create(url, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_create");

    xthread_handle hThread;
    hThread = xthread_create(_thread, eye_tracker);

    tobiigaze_connect(eye_tracker, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_connect");
    // Good habit to start by retrieving device info to check that communication works.
    print_device_info(eye_tracker);

    printf("Get ready for calibration...\n");
    XSLEEP(2000);

    // Initialize before each new calibration
    _next_calibration_point = 0;
    _calibration_done = 0;
    _calibration_failed = 0;

    // calibration
    tobiigaze_calibration_start_async(eye_tracker, add_calibration_point_handler, eye_tracker);

    printf("Waiting for calibration to finish...\n");
    while (!_calibration_done)
        XSLEEP(100);

    printf("Calibration %s.\n", _calibration_failed ? "FAILED" : "OK");

    tobiigaze_disconnect_async(eye_tracker, &on_disconnect_callback, eye_tracker);

    // give some time to disconnect gracefully. In a real-world scenario, a mutex should be used here instead
    XSLEEP(200);

    tobiigaze_break_event_loop(eye_tracker);
    xthread_join(hThread);

    tobiigaze_destroy(eye_tracker);

    return 0;
}
