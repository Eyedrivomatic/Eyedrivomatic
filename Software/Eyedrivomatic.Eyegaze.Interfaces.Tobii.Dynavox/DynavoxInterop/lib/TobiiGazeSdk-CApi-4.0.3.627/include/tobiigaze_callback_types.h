//  Copyright 2013 Tobii Technology AB. All rights reserved.
#pragma once
#include "tobiigaze_define.h"
#include "tobiigaze_data_types.h"
#include "tobiigaze_error_codes.h"

#ifdef __cplusplus
extern "C" {
#endif

    /**
     * This type is used for the callback function that is registered with tobiigaze_add_gaze_data_listener. The callback function will be called when gaze
     * data is received from the eye tracker.
     * @param gaze_data             The received Gaze Data.
     * @param gaze_data_extensions  Contains optional extension data. Does not contain any data on standard eye trackers.
     * @param user_data             Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_gaze_listener)(const struct tobiigaze_gaze_data *gaze_data, const struct tobiigaze_gaze_data_extensions *gaze_data_extensions, void *user_data);

    /**
     * This type is used for callback functions that are registered with several asynchronous commands that do not have any return data.
     * The callback function will be called when the command is completed.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_callback)(tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for callback functions that are registered with several asynchronous commands that do not have any return data or an error code.
     * The callback function will be called when the command is completed.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_basic_callback)(void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_get_display_area_async. The callback function will be called
     * when the command is completed.
     * @param display_area  The retrieved Display Area.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_display_area_callback)(const struct tobiigaze_display_area *display_area, tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_get_device_info_async. The callback function will be called
     * when the command is completed.
     * @param device_info   The retrieved device info.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_device_info_callback)(const struct tobiigaze_device_info *device_info, tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_get_track_box_async. The callback function will be called
     * when the command is completed.
     * @param track_box     The retrieved Track Box.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_trackbox_callback)(const struct tobiigaze_track_box *track_box, tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_get_geometry_mounting_async. The callback function will be called
     * when the command is completed.
     * @param geometry_mounting     The retrieved geometry mounting.
     * @param error_code            Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data             Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_geometry_mounting_callback)(const struct tobiigaze_geometry_mounting *geometry_mounting, tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_get_calibration_async. The callback function will be called
     * when the command is completed.
     * @param calibration   The retrieved Calibration.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_calibration_callback)(const struct tobiigaze_calibration *calibration, tobiigaze_error_code error_code, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_register_key_provider. The callback function will be called
     * when an eye tracker is to be unlocked.
     * @param realm_id      The realm of the eye tracker to provide the key for.
     * @param key           The key to use for unlocking the eye tracker.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_key_provider_callback)(uint32_t realm_id, struct tobiigaze_key *key, void *user_data);

    /**
     * This callback is used for timestamping gaze data when time sync is enabled.
     * @param timestamp     The timestamp that will be used for time synchronization. Should be returned in microseconds.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_gettime_callback)(uint64_t *timestamp, void *user_data);

    /**
     * This type is used for the callback function that is registered with tobiigaze_send_custom_command_async. The callback function will be called
     * when an eye tracker is to be unlocked.
     * @param command       The response command from the eye tracker.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    typedef void (TOBIIGAZE_CALL *tobiigaze_async_custom_command_callback)(const struct tobiigaze_custom_command *command, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_framerates_callback)(const struct tobiigaze_framerates *frame_rates, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_framerate_callback)(const float *frame_rate, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_illuminations_callback)(const struct tobiigaze_illumination_modes *modes, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_illumination_callback)(const struct tobiigaze_illumination_mode *mode, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_diagonstics_report_callback)(const struct tobiigaze_blob *blob, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_low_blink_mode_callback)(const uint32_t *low_blink, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_unit_name_callback)(const struct tobiigaze_unit_name *unit_name, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_extensions_callback)(const struct tobiigaze_extensions *extensions, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_config_key_value_callback)(const struct tobiigaze_config_key_value *config_key_value, tobiigaze_error_code error_code, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_timesync_receive_packet_callback)(const struct tobiigaze_timesync_info *packet, void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_timesync_send_packet_callback)(void *user_data);

    typedef void (TOBIIGAZE_CALL *tobiigaze_async_xconfig_writable_callback)(const uint32_t *xconfig_writable, tobiigaze_error_code error_code, void *user_data);

#ifdef __cplusplus
}
#endif
