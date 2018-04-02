//  Copyright 2013 Tobii Technology AB. All rights reserved.
#pragma once

typedef enum
{
    // Success
    TOBIIGAZE_ERROR_SUCCESS =                               0,

    // Generic errors
    TOBIIGAZE_ERROR_UNKNOWN =                               1,
    TOBIIGAZE_ERROR_OUT_OF_MEMORY =                         2,
    TOBIIGAZE_ERROR_BUFFER_TOO_SMALL =                      3,
    TOBIIGAZE_ERROR_INVALID_PARAMETER =                     4,

    // Sync function errors
    TOBIIGAZE_ERROR_TIMEOUT =                               100,
    TOBIIGAZE_ERROR_OPERATION_ABORTED =                     101,

    // Transport errors
    TOBIIGAZE_ERROR_INVALID_URL =                           200,
    TOBIIGAZE_ERROR_ENDPOINT_NAME_LOOKUP_FAILED =           201,
    TOBIIGAZE_ERROR_ENDPOINT_CONNECT_FAILED =               202,
    TOBIIGAZE_ERROR_DEVICE_COMMUNICATION_ERROR =            203,
    TOBIIGAZE_ERROR_ALREADY_CONNECTED =                     204,
    TOBIIGAZE_ERROR_NOT_CONNECTED =                         205,
    TOBIIGAZE_ERROR_TIMESYNC_COMMUNICATION_ERROR =          206,

    // Protocol errors
    TOBIIGAZE_ERROR_PROTOCOL_DECODING_ERROR =               300,
    TOBIIGAZE_ERROR_PROTOCOL_VERSION_ERROR =                301,

    // Errors from eye tracker firmware
    TOBIIGAZE_FW_ERROR_UNKNOWN_OPERATION =                  0x20000500,
    TOBIIGAZE_FW_ERROR_UNSUPPORTED_OPERATION =              0x20000501,
    TOBIIGAZE_FW_ERROR_OPERATION_FAILED =                   0x20000502,
    TOBIIGAZE_FW_ERROR_INVALID_PAYLOAD =                    0x20000503,
    TOBIIGAZE_FW_ERROR_UNKNOWN_ID =                         0x20000504,
    TOBIIGAZE_FW_ERROR_UNAUTHORIZED =                       0x20000505,
    TOBIIGAZE_FW_ERROR_EXTENSION_REQUIRED =                 0x20000506,
    TOBIIGAZE_FW_ERROR_INTERNAL_ERROR =                     0x20000507,
    TOBIIGAZE_FW_ERROR_STATE_ERROR =                        0x20000508,
    TOBIIGAZE_FW_ERROR_INVALID_PARAMETER =                  0x20000509,
    TOBIIGAZE_FW_ERROR_OPERATION_ABORTED =                  0x2000050A
} tobiigaze_error_code;
