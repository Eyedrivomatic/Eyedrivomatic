//  Copyright 2013 Tobii Technology AB. All rights reserved.
#pragma once

#include <stdint.h>
#include <stddef.h>
#include "tobiigaze_data_types.h"
#include "tobiigaze_error_codes.h"
#include "tobiigaze_callback_types.h"
#include "tobiigaze_define.h"

#ifdef __cplusplus
extern "C" {
#endif

    /**
     * Creates an eye tracker instance.
     * @param url           An url identifying the eye tracker. Currently only the tet-tcp protocol is defined. Example: "tet-tcp://172.68.195.1".
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @return              An eye tracker instance, or NULL if creation failed.
     */
    TOBIIGAZE_API tobiigaze_eye_tracker* TOBIIGAZE_CALL tobiigaze_create(const char *url, tobiigaze_error_code *error_code);

    /**
     * Destroys an eye tracker instance. Must NOT be called from a callback.
     * @param eye_tracker   An eye tracker instance.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_destroy(tobiigaze_eye_tracker *eye_tracker);

    /**
     * Registers a callback that will return an error code when a spontaneous error occurs (an error not directly associated with a command). Most likely
     * this error is related to problems with the eye tracker communication and is unrecoverable.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called when an error occurs. Set to NULL to unregister a previously registered callback.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_register_error_callback(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Gets the version of the library.
     * @return   The version of the library on the form "1.0.2".
     */
    TOBIIGAZE_API const char* TOBIIGAZE_CALL tobiigaze_get_version();

    /**
     * Sets the logging output filename and verbosity.
     * @param filename              The filename of the logfile.
     * @param log_level             The verbosity of the logging.
     * @param error_code            Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_logging(const char *filename, tobiigaze_log_level log_level, tobiigaze_error_code *error_code);

    /**
     * Connects to an eye tracker asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_connect_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Connects to an eye tracker synchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_connect(tobiigaze_eye_tracker *eye_tracker, tobiigaze_error_code *error_code);

    /**
     * Disonnects from an eye tracker asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_disconnect_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_basic_callback callback, void *user_data);

    /**
     * Disconnects from an eye tracker synchronously.
     * @param eye_tracker   An eye tracker instance.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_disconnect(tobiigaze_eye_tracker *eye_tracker);

    /**
     * Runs the event loop. This is a blocking call and must be called on a dedicated thread.
     * @param eye_tracker   An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_run_event_loop(tobiigaze_eye_tracker *eye_tracker, tobiigaze_error_code *error_code);

    /**
     * Runs the event loop on asynchronously. That is, it creates a thread internally on which the event loops run.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called when the run loop exits. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_run_event_loop_on_internal_thread(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Breaks the event loop. This will make the blocking tobiigaze_run_event_loop call return. Must NOT be called from a callback. Any outstanding work is
     * cancelled. If run_event_loop was used to start the loop, it is up to the client to synchronize the thread with join(). If run_event_loop_async was
     * used, the thread is joined in this call. After this function has been called, it is not possible to start a new event loop on the same eye tracker instance.
     * @param eye_tracker   An eye tracker instance.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_break_event_loop(tobiigaze_eye_tracker *eye_tracker);

    /**
     * Starts gaze tracking asynchronously.
     * @param eye_tracker     An eye tracker instance.
     * @param callback        A callback function that will be called on command completion (note that this is not the callback that will provide the actual gaze data).
     * @param gaze_callback   A callback function that will be called asynchronously when gaze data is available.
     * @param user_data       Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_start_tracking_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, tobiigaze_gaze_listener gaze_callback, void *user_data);

    /**
     * Starts gaze tracking synchronously.
     * @param eye_tracker    An eye tracker instance.
     * @param gaze_callback  A callback function that will be called asynchronously when gaze data is available.
     * @param error_code     Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data      Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_start_tracking(tobiigaze_eye_tracker *eye_tracker, tobiigaze_gaze_listener gaze_callback, tobiigaze_error_code *error_code, void *user_data);

    /**
     * Stops gaze tracking asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_stop_tracking_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Stops gaze tracking synchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_stop_tracking(tobiigaze_eye_tracker *eye_tracker, tobiigaze_error_code *error_code);

    /**
     * Gets the device info, such as platform, versions etc, asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_device_info_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_device_info_callback callback, void *user_data);

    /**
     * Gets the device info, such as platform, versions etc, synchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param device_info   Device information out parameter.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_device_info(tobiigaze_eye_tracker *eye_tracker, struct tobiigaze_device_info *device_info, tobiigaze_error_code *error_code);

    /**
     * Gets the track box asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_track_box_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_trackbox_callback callback, void *user_data);

    /**
     * Gets the track box synchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param track_box     Track box information out parameter.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_track_box(tobiigaze_eye_tracker *eye_tracker, struct tobiigaze_track_box *track_box, tobiigaze_error_code *error_code);

    /**
     * Gets the geometry mounting asynchronously.
     * @param eye_tracker           An eye tracker instance.
     * @param callback              A callback function that will be called on command completion.
     * @param user_data             Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_geometry_mounting_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_geometry_mounting_callback callback, void *user_data);

    /**
     * Gets the geometry mounting.
     * @param eye_tracker           An eye tracker instance.
     * @param geometry_mounting     Geometry mounting out parameter.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_geometry_mounting(tobiigaze_eye_tracker *eye_tracker, struct tobiigaze_geometry_mounting *geometry_mounting, tobiigaze_error_code *error_code);

    /**
     * Registers a callback providing a key for unlocking the eye tracker. The Tobii Gaze Core library unlocks developer edition
     * eye trackers automatically; this function can be used to unlock other eye trackers. Registering a key provider disables the built-in default key.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function. Set to NULL to unregister a previously registered callback, that is, use the default key provider.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_register_key_provider(tobiigaze_eye_tracker *eye_tracker, tobiigaze_key_provider_callback callback, tobiigaze_error_code *error_code, void *user_data);

    /**
     * Gets the url associated with the eye tracker instance.
     * @param eye_tracker   An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @return              A string containing the url.
     */
    TOBIIGAZE_API const char* TOBIIGAZE_CALL tobiigaze_get_url(tobiigaze_eye_tracker *eye_tracker, tobiigaze_error_code *error_code);

    /**
     * Returns the connection status to the eye tracker.
     * @return              1 if the eye tracker is connected, otherwise 0.
     */
    TOBIIGAZE_API int TOBIIGAZE_CALL tobiigaze_is_connected(tobiigaze_eye_tracker *eye_tracker);

    /**
     * Returns the meaning of an error code.
     * @param error_code    An error code returned from TobiiGazeCore.
     * @return              A string description of the specified error code.
     */
    TOBIIGAZE_API const char* TOBIIGAZE_CALL tobiigaze_get_error_message(tobiigaze_error_code error_code);

#ifdef __cplusplus
}
#endif
