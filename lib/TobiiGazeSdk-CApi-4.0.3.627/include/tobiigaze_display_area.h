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
     * Get the display area of the device asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_display_area_async(tobiigaze_eye_tracker *eye_tracker, tobiigaze_async_display_area_callback callback, void *user_data);

    /**
     * Sets the display area of the device asynchronously.
     * This call will result in a TOBII_FW_ERROR_UNSUPPORTED_OPERATION, in the callback, if the connected Eye Tracker does not support the operation.
     * @param eye_tracker   An eye tracker instance.
     * @param display_area  A struct containing the display area coordinates.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_display_area_async(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_display_area *display_area, tobiigaze_async_callback callback, void *user_data);

    /**
     * Gets the display area synchronously.
     * @param eye_tracker     An eye tracker instance.
     * @param configuration   A struct containing the current display area coordinates (out parameter).
     * @param error_code      Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_display_area(tobiigaze_eye_tracker *eye_tracker, struct tobiigaze_display_area *display_area, tobiigaze_error_code *error_code);

    /**
     * Sets the display area synchronously.
     * This call will result in a TOBII_FW_ERROR_UNSUPPORTED_OPERATION if the connected Eye Tracker does not support the operation.
     * @param eye_tracker     An eye tracker instance.
     * @param configuration   A struct containing the display area coordinates that should be set.
     * @param error_code              Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_display_area(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_display_area *display_area, tobiigaze_error_code *error_code);

#ifdef __cplusplus
}
#endif
