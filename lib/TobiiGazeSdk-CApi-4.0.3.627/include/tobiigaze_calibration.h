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
     * Acquires the calibration state and clears the temporary calibration buffer.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_calibration_start_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Releases the calibration state. This should always be done when the calibration is completed.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_calibration_stop_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Adds data to the temporary calibration buffer for the specified calibration point which the user is assumed to be looking at.
     * @param eye_tracker   An eye tracker instance.
     * @param point         A two dimensional point specified in the ADCS coordinate system (screen size percentage) where the users gaze is expected to be looking.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_calibration_add_point_async(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_point_2d *point, tobiigaze_async_callback callback, void *user_data);

    /**
     * Removes the data associated with the specified calibration point from the temporary calibration buffer.
     * This is normally done when recalibrating a point with bad quality data preceding a new call to tobiigaze_calibration_add_point_async with the same point.
     * @param eye_tracker   An eye tracker instance.
     * @param point         A two dimensional point specified in the ADCS coordinate system (screen size percentage) which has previously been added to the calibration.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_calibration_remove_point_async(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_point_2d *point, tobiigaze_async_callback callback, void *user_data);

    /**
     * Computes a calibration based on data in the temporary calibration buffer. If this operation succeeds the temporary calibration buffer will be copied to the active calibration buffer.
     * If there is insufficient data to compute a calibration, TOBIIGAZE_FW_ERROR_OPERATION_FAILED will be returned via the callback.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_calibration_compute_and_set_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_callback callback, void *user_data);

    /**
     * Gets current calibration from the active calibration buffer.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_calibration_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_calibration_callback callback, void *user_data);

    /**
     * Sets the specified calibration as the active calibration. The calibration data is copied both to the active and the temporary calibration buffers.
     * @param eye_tracker   An eye tracker instance.
     * @param calibration   A struct containing calibration data.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_calibration_async(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_calibration *calibration, tobiigaze_async_callback callback, void *user_data);

    /**
     * Gets current calibration from the active calibration buffer.
     * @param eye_tracker   An eye tracker instance.
     * @param calibration   Calibration out parameter.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_calibration(tobiigaze_eye_tracker *eye_tracker, struct tobiigaze_calibration *calibration, tobiigaze_error_code *error_code);

    /**
     * Sets the specified calibration as the active calibration. The calibration data is copied both to the active and the temporary calibration buffers.
     * @param eye_tracker   An eye tracker instance.
     * @param calibration   A struct containing the calibration.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_calibration(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_calibration *calibration, tobiigaze_error_code *error_code);

    /**
     * Retrieves individual calibration point data from the specified calibration.
     * @param calibration                  A struct containing the calibration data.
     * @param point_data_items             An array to be filled with point data entries.
     * @param point_datas_items_capacity   The capacity of the point data array (maximum number of entries).
     * @param point_datas_items_size       The number of point data entries written to the array. Max items is TOBIIGAZE_MAX_CALIBRATION_POINT_DATA_ENTRIES.
     * @param error_code                   Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_calibration_point_data_items(const struct tobiigaze_calibration *calibration, struct tobiigaze_calibration_point_data *point_data_items, uint32_t point_data_items_capacity, uint32_t *point_data_items_size, tobiigaze_error_code *error_code);


#ifdef __cplusplus
}
#endif
