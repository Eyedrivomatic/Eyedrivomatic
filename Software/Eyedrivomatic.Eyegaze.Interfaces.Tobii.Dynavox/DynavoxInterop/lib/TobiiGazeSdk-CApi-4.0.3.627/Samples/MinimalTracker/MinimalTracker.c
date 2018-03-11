/*
 * Copyright 2013 Tobii Technology AB. All rights reserved.
 */

#include <stdio.h>
#include <stdlib.h>
#include "Common.h"
#include "tobiigaze_discovery.h"

#define URL_SIZE 256

/*
 * This is a simple example that demonstrates the synchronous TobiiGazeCore calls.
 * It prints gaze data for 20 seconds.
 */

// Prints gaze information, or "-" if gaze position could not be determined.
void on_gaze_data(const struct tobiigaze_gaze_data* gazedata, const struct tobiigaze_gaze_data_extensions* extensions, void *user_data)
{
    printf("%20.3f ", gazedata->timestamp / 1e6); // in seconds
    printf("%d ", gazedata->tracking_status);

    if (gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_LEFT) {
        printf("[%7.4f,%7.4f] ", gazedata->left.gaze_point_on_display_normalized.x, gazedata->left.gaze_point_on_display_normalized.y);
    }
    else {
        printf("[%7s,%7s] ", "-", "-");
    }

    if (gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED ||
        gazedata->tracking_status == TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_RIGHT) {
        printf("[%7.4f,%7.4f] ", gazedata->right.gaze_point_on_display_normalized.x, gazedata->right.gaze_point_on_display_normalized.y);
    }
    else {
        printf("[%7s,%7s] ", "-", "-");
    }

    printf("\n");
}

// Queries for and prints device information.
void print_device_info(tobiigaze_eye_tracker* eye_tracker)
{
    tobiigaze_error_code error_code;
    struct tobiigaze_device_info info;

    tobiigaze_get_device_info(eye_tracker, &info, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_get_device_info");

    printf("Serial number: %s\n", info.serial_number);
}

int main(int argc, char** argv)
{
    char url[URL_SIZE];
    tobiigaze_error_code error_code;
    tobiigaze_eye_tracker* eye_tracker;

    // Process command-line arguments.
    if (argc == 2) {
        strncpy(url, argv[1], URL_SIZE);

        if (!strcmp(url, "--auto")) {
            tobiigaze_get_connected_eye_tracker(url, URL_SIZE, &error_code);
            if (error_code) {
                printf("No eye tracker found.\n");
                exit(-1);
            }
        }
    }
    else {
        printf("usage: MinimalTracker {url|--auto}\n");
        printf("example: MinimalTracker tet-tcp://172.28.195.1\n");
        return 0;
    }

    printf("TobiiGazeCore DLL version: %s\n", tobiigaze_get_version());

    // Create an eye tracker instance.
    printf("Creating eye tracker with url %s.\n", url);
    eye_tracker = tobiigaze_create(url, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_create");

    // Start the event loop. This must be done before connecting.
    tobiigaze_run_event_loop_on_internal_thread(eye_tracker, 0, 0);

    // Connect to the tracker.
    tobiigaze_connect(eye_tracker, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_connect");
    printf("Connected.\n");
    print_device_info(eye_tracker);

    // Track for a while.
    tobiigaze_start_tracking(eye_tracker, &on_gaze_data, &error_code, 0);
    report_and_exit_on_error(error_code, "tobiigaze_start_tracking");
    printf("Tracking started.\n");

    XSLEEP(20000);

    tobiigaze_stop_tracking(eye_tracker, &error_code);
    report_and_exit_on_error(error_code, "tobiigaze_stop_tracking");
    printf("Tracking stopped.\n");

    // Disconnect and clean up.
    tobiigaze_disconnect(eye_tracker);
    printf("Disconnected.\n");
    tobiigaze_break_event_loop(eye_tracker);
    tobiigaze_destroy(eye_tracker);

    return 0;
}
