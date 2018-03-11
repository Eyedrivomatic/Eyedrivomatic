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
     * Sends a custom command to the eye tracker
     * @param eye_tracker   An eye tracker instance.
     * @param in_data       Data to send to the eye tracker. The data must be allocated by the caller.
     * @param out_data      Data to receive from the eye tracker.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_send_custom_command(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_custom_command *in_data, struct tobiigaze_custom_command *out_data, tobiigaze_error_code *error_code);

    /**
     * Sends a custom command to the eye tracker asynchronously.
     * @param eye_tracker   An eye tracker instance.
     * @param in_data       Data to send to the eye tracker. The data must be allocated by the caller.
     * @param callback      A callback function that will be called on command completion.
     * @param user_data     Optional user supplied data that will be passed unmodified to the callback function. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_send_custom_command_async(tobiigaze_eye_tracker *eye_tracker, const struct tobiigaze_custom_command *in_data, tobiigaze_async_custom_command_callback callback, void *user_data);

    /**
     * Sets an API option. Options are documented in tobiigaze_option.
     * @param eye_tracker   An eye tracker instance.
     * @param option        The id of the option.
     * @param value         A pointer to the value of the option.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_set_option(tobiigaze_eye_tracker *eye_tracker, tobiigaze_option option, void *value, tobiigaze_error_code *error_code);

    /**
     * Translates raw extension bytes to an integer. A type-check will occur, and an error will be returned if the types do not match.
     * @param extension     An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @return              The integer value of the extension field.
     */
    TOBIIGAZE_API uint32_t TOBIIGAZE_CALL tobiigaze_gaze_data_extension_to_uint32(const struct tobiigaze_gaze_data_extension *extension, tobiigaze_error_code *error_code);

    /**
     * Translates raw extension bytes to a float. A type-check will occur, and an error will be returned if the types do not match.
     * @param extension     An eye tracker instance.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     * @return              The float value of the extension field.
     */
    TOBIIGAZE_API float TOBIIGAZE_CALL tobiigaze_gaze_data_extension_to_float(const struct tobiigaze_gaze_data_extension *extension, tobiigaze_error_code *error_code);

    /**
     * Translates raw extension bytes to a string. A type-check will occur, and an error will be returned if the types do not match.
     * @param extension     An eye tracker instance.
     * @param destination   A buffer that will contain the data.
     * @param capacity      The capacity of the destination buffer.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_gaze_data_extension_to_string(const struct tobiigaze_gaze_data_extension *extension, char *destination, uint32_t capacity, tobiigaze_error_code *error_code);

    /**
     * Translates raw extension bytes to a blob. A type-check will occur, and an error will be returned if the types do not match.
     * @param extension     An eye tracker instance.
     * @param destination   A buffer that will contain the data.
     * @param capacity      The capacity of the destination buffer.
     * @param error_code    Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_gaze_data_extension_to_blob(const struct tobiigaze_gaze_data_extension *extension, uint8_t *destination, uint32_t capacity, tobiigaze_error_code *error_code);


#ifdef __cplusplus
}
#endif
