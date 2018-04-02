//  Copyright 2013 Tobii Technology AB. All rights reserved.
#pragma once

#include <stdint.h>
#include <stddef.h>

typedef struct tobiigaze_eye_tracker tobiigaze_eye_tracker;

/**
 * This enum hold the different log levels.
 */
typedef enum
{
    TOBIIGAZE_LOG_LEVEL_OFF = 0,
    TOBIIGAZE_LOG_LEVEL_DEBUG = 1,
    TOBIIGAZE_LOG_LEVEL_INFO = 2,
    TOBIIGAZE_LOG_LEVEL_WARNING = 3,
    TOBIIGAZE_LOG_LEVEL_ERROR = 4,
} tobiigaze_log_level;

/**
* This enum hold various tobiigaze constants.
*/
typedef enum
{
    TOBIIGAZE_DEVICE_INFO_MAX_SERIAL_NUMBER_LENGTH = 128,
    TOBIIGAZE_DEVICE_INFO_MAX_MODEL_LENGTH = 64,
    TOBIIGAZE_DEVICE_INFO_MAX_GENERATION_LENGTH = 64,
    TOBIIGAZE_DEVICE_INFO_MAX_FIRMWARE_LENGTH = 128,
    TOBIIGAZE_CALIBRATION_DATA_CAPACITY = 4 * 1024 * 1024,
    TOBIIGAZE_KEY_SIZE = 32,
    TOBIIGAZE_MAX_CALIBRATION_POINT_DATA_ITEMS = 512,
    TOBII_USB_DEVICE_INFO_MAX_SIZE = 128,
    TOBII_USB_DEVICE_ADDRESS_MAX_SIZE = 138,
    TOBII_USB_MAX_DEVICES = 9,
    TOBIIGAZE_FRAMERATES_MAX_SIZE = 32,
    TOBIIGAZE_ILLUMINATION_MODE_STRING_MAX_SIZE = 64,
    TOBIIGAZE_ILLUMINATION_MODES_MAX_SIZE = 16,
    TOBIIGAZE_UNIT_NAME_MAX_SIZE = 64,
    TOBIIGAZE_EXTENSION_NAME_MAX_SIZE = 16,
    TOBIIGAZE_EXTENSIONS_MAX_SIZE = 16,
    TOBIIGAZE_MAX_WAKE_ON_GAZE_REGIONS = 4,
    TOBIIGAZE_AUTHORIZE_CHALLENGE_MAX_LEN = 512,
    TOBIIGAZE_MAX_GAZE_DATA_EXTENSIONS = 32,
    TOBIIGAZE_MAX_GAZE_DATA_EXTENSION_LENGTH = 256,
    TOBIIGAZE_MAX_CONFIG_KEY_LENGTH = 128
} tobiigaze_constants;

/**
* This enum hold the possible gaze tracking statuses.
*/
typedef enum
{
    TOBIIGAZE_TRACKING_STATUS_NO_EYES_TRACKED = 0,
    TOBIIGAZE_TRACKING_STATUS_BOTH_EYES_TRACKED = 1,
    TOBIIGAZE_TRACKING_STATUS_ONLY_LEFT_EYE_TRACKED = 2,
    TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_LEFT = 3,
    TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_UNKNOWN_WHICH = 4,
    TOBIIGAZE_TRACKING_STATUS_ONE_EYE_TRACKED_PROBABLY_RIGHT = 5,
    TOBIIGAZE_TRACKING_STATUS_ONLY_RIGHT_EYE_TRACKED = 6
} tobiigaze_tracking_status;

/**
 * This enum hold the possible calibration point statuses.
 */
typedef enum
{
  TOBIIGAZE_CALIBRATION_POINT_STATUS_FAILED_OR_INVALID = -1,
  TOBIIGAZE_CALIBRATION_POINT_STATUS_VALID_BUT_NOT_USED_IN_CALIBRATION = 0,
  TOBIIGAZE_CALIBRATION_POINT_STATUS_VALID_AND_USED_IN_CALIBRATION = 1
} tobiigaze_calibration_point_status;

/**
 * This enum holds settable options.
 * TOBIIGAZE_OPTION_TIMEOUT:    Timeout for synchronous operations. Value is of type uint32_t.
 */
typedef enum
{
    TOBIIGAZE_OPTION_TIMEOUT = 0
} tobiigaze_option;

/**
* This struct holds Device Info that is fetched from the Eye Tracker. The char arrays holds null
* terminated strings.
* @field serial_number The serial number of the eye tracker.
* @field model The eye tracker model, e.g. "REX_DEV_Laptop".
* @field generation The eye tracker generation, e.g. G5.
* @field firmware_version The eye tracker serial number.
*/
struct tobiigaze_device_info
{
    char serial_number[TOBIIGAZE_DEVICE_INFO_MAX_SERIAL_NUMBER_LENGTH];
    char model[TOBIIGAZE_DEVICE_INFO_MAX_MODEL_LENGTH];
    char generation[TOBIIGAZE_DEVICE_INFO_MAX_GENERATION_LENGTH];
    char firmware_version[TOBIIGAZE_DEVICE_INFO_MAX_FIRMWARE_LENGTH];
};

/**
* This struct holds eye tracker Calibration Data that is fetched from or sent to the Eye Tracker. The data array holds null
* terminated strings.
* @field data The calibration data.
* @field actual_size The length of the calibration data.
*/
struct tobiigaze_calibration
{
    uint8_t data[TOBIIGAZE_CALIBRATION_DATA_CAPACITY];
    uint32_t actual_size;
};

/**
 * This struct holds a two dimensional point.
 * @field x X coordinate.
 * @field y Y coordinate.
 */
struct tobiigaze_point_2d
{
    double x;
    double y;
};

/**
 * This struct holds a two dimensional point.
 * @field x X coordinate.
 * @field y Y coordinate.
 */
struct tobiigaze_point_2d_f
{
    float x;
    float y;
};

/**
 * This struct holds a three dimensional point.
 * @field x X coordinate.
 * @field y Y coordinate.
 * @field z Z coordinate.
 */
struct tobiigaze_point_3d
{
    double x;
    double y;
    double z;
};

/**
 * This struct holds a rectangle.
 * @field left Specifies the x-coordinate of the upper-left corner of a rectangle.
 * @field top Specifies the y-coordinate of the upper-left corner of a rectangle.
 * @field right Specifies the x-coordinate of the lower-right corner of a rectangle.
 * @field bottom Specifies the y-coordinate of the lower-right corner of a rectangle.
 */
struct tobiigaze_rect
{
    int32_t left;
    int32_t top;
    int32_t right;
    int32_t bottom;
};

/**
* This struct holds gaze data for one eye.
* @field eye_position_from_eye_tracker_mm
* @field eye_position_in_track_box_normalized
* @field gaze_point_from_eye_tracker_mm
* @field gaze_point_on_display_normalized
*/
struct tobiigaze_gaze_data_eye
{
    struct tobiigaze_point_3d eye_position_from_eye_tracker_mm;
    struct tobiigaze_point_3d eye_position_in_track_box_normalized;
    struct tobiigaze_point_3d gaze_point_from_eye_tracker_mm;
    struct tobiigaze_point_2d gaze_point_on_display_normalized;
};

/**
* This struct holds gaze data reveiced from the eye tracker.
* @field timestamp Timestamp for the gaze data
* @field tracking_status The combined tracking status for both eyes.
* @field left Gaze data for the left eye
* @field right Gaze data for the right eye
*/
struct tobiigaze_gaze_data
{
    uint64_t timestamp;
    tobiigaze_tracking_status tracking_status;
    struct tobiigaze_gaze_data_eye left;
    struct tobiigaze_gaze_data_eye right;
};

/**
* This struct holds a gaze data extension.
* @field column_id The id of the extension which uniquely identifies it.
* @field data The extension data. Use the helper functions in tobiigaze_ext.h to convert this data to types.
* @field actual_size The size of the data.
*/
struct tobiigaze_gaze_data_extension
{
    uint32_t column_id;
    uint8_t data[TOBIIGAZE_MAX_GAZE_DATA_EXTENSION_LENGTH];
    uint32_t actual_size;
};

/**
* This struct holds a gaze data extension.
* @field extensions An array of extensions.
* @field actual_size The number of extensions.
*/
struct tobiigaze_gaze_data_extensions
{
    struct tobiigaze_gaze_data_extension extensions[TOBIIGAZE_MAX_GAZE_DATA_EXTENSIONS];
    uint32_t actual_size;
};

/**
* This struct holds a track box: a frustum specified by eight points in space.
* Front means closer to the eye tracker.
* Left and right are as seen by the user.
*
* @field front_upper_right_point Front upper right point.
* @field front_upper_left_point Front upper left point.
* @field front_lower_left_point Front lower left point.
* @field front_lower_right_point Front lower right point.
* @field back_upper_right_point Back upper right point.
* @field back_upper_left_point Back upper left point.
* @field back_lower_left_point Back lower left point.
* @field back_lower_right_point Back lower right point.
*/
struct tobiigaze_track_box
{
    struct tobiigaze_point_3d front_upper_right_point;
    struct tobiigaze_point_3d front_upper_left_point;
    struct tobiigaze_point_3d front_lower_left_point;
    struct tobiigaze_point_3d front_lower_right_point;
    struct tobiigaze_point_3d back_upper_right_point;
    struct tobiigaze_point_3d back_upper_left_point;
    struct tobiigaze_point_3d back_lower_left_point;
    struct tobiigaze_point_3d back_lower_right_point;
};

/**
* This struct holds a display area.
* Left and right are as seen by the user.
*
* @field upperLeft Upper left point.
* @field upperRight Upper right point.
* @field lowerLeft Lower left point.
*/
struct tobiigaze_display_area
{
    struct tobiigaze_point_3d top_left;
    struct tobiigaze_point_3d top_right;
    struct tobiigaze_point_3d bottom_left;
};

/**
 * This struct holds a key for unlocking an eye tracker.
 * @field data The key.
 */
struct tobiigaze_key
{
    uint8_t data[TOBIIGAZE_KEY_SIZE];
};

/**
 * Contains data about a calibration point sample.
 * @field true_position       The point in normalized coordinates on the display area where the calibration stimulus was displayed.
 * @field left_map_position   The left eye gaze point in normalized coordinates on the display area after calibration.
 * @field left_status         Status code containing information about the validity and usage of the left eye data.
 * @field right_map_position  The right eye gaze point in normalized coordinates on the display area after calibration.
 * @field right_status        Status code containing information about the validity and usage of the right eye data.
 */
struct tobiigaze_calibration_point_data
{
    struct tobiigaze_point_2d_f true_position;

    struct tobiigaze_point_2d_f left_map_position;
    tobiigaze_calibration_point_status left_status;

    struct tobiigaze_point_2d_f right_map_position;
    tobiigaze_calibration_point_status right_status;
};

/**
 * This structs hold discovery information for USB trackers.
 */
struct usb_device_info
{
    char serialNumber[TOBII_USB_DEVICE_INFO_MAX_SIZE];
    char productName[TOBII_USB_DEVICE_INFO_MAX_SIZE];
    char platformType[TOBII_USB_DEVICE_INFO_MAX_SIZE];
    char firmwareVersion[TOBII_USB_DEVICE_INFO_MAX_SIZE];
};

/**
 * This structs hold information about available frame rates.
 */
struct tobiigaze_framerates
{
    float framerates[TOBIIGAZE_FRAMERATES_MAX_SIZE];
    uint32_t actual_size;
};

/**
 * Contains an illumination mode identifier.
 */
struct tobiigaze_illumination_mode
{
    char data[TOBIIGAZE_ILLUMINATION_MODE_STRING_MAX_SIZE];
};

/**
 * This struct contains a list of all supported illumination modes.
 */
struct tobiigaze_illumination_modes
{
    char data[TOBIIGAZE_ILLUMINATION_MODES_MAX_SIZE][TOBIIGAZE_ILLUMINATION_MODE_STRING_MAX_SIZE];
    uint32_t actual_size;
};

/**
 * This structs contains data for sending and receiving custom commands.
 * @field data must be allocated by the client.
 */
struct tobiigaze_custom_command
{
    uint8_t *data;
    uint32_t capacity;
    uint32_t actual_size;
};

/**
 * This structs holds the given name of a unit.
 */
struct tobiigaze_unit_name
{
    char data[TOBIIGAZE_UNIT_NAME_MAX_SIZE];
};

/**
 * This struct holds a diagonstics image.
 */
struct tobiigaze_blob
{
    uint8_t data[TOBIIGAZE_CALIBRATION_DATA_CAPACITY];
    uint32_t actual_size;
};

/**
 * This struct holds information about an extension.
 */
struct tobiigaze_extension
{
    uint32_t protocol_version;
    uint32_t extension_id;
    uint32_t realm;
    char name[TOBIIGAZE_EXTENSION_NAME_MAX_SIZE];
};

/**
 * This struct holds a list of extensions.
 */
struct tobiigaze_extensions
{
    struct tobiigaze_extension extensions[TOBIIGAZE_EXTENSIONS_MAX_SIZE];
    uint32_t actual_size;
};

struct tobiigaze_wake_on_gaze_region
{
	struct tobiigaze_point_2d upper_left;
	struct tobiigaze_point_2d lower_right;
};

struct tobiigaze_wake_on_gaze_configuration
{
	uint32_t dwell_time; // milliseconds
	struct tobiigaze_wake_on_gaze_region region[TOBIIGAZE_MAX_WAKE_ON_GAZE_REGIONS];
	uint32_t actual_size;
};

struct tobiigaze_geometry_mounting
{
  uint32_t guides;
  float width;
  float angle;
  struct tobiigaze_point_3d external_offset;
  struct tobiigaze_point_3d internal_offset;
};

struct tobiigaze_authorize_challenge
{
	uint32_t    realm_id;
	uint32_t    algorithm;
	uint8_t		challenge[TOBIIGAZE_AUTHORIZE_CHALLENGE_MAX_LEN];
	uint32_t	actual_size;
};

struct tobiigaze_pay_per_use_info
{
	uint32_t is_ppu_unit;
	uint32_t realm_id;
	uint32_t realm_is_authorized;
};

/**
 * This struct holds timesync information.
 */
struct tobiigaze_timesync_info
{
    uint32_t packet_number;
    uint64_t remote_timestamp;
};

/**
 * This struct holds configuration information.
 */
struct tobiigaze_config_key_value
{
    char key[TOBIIGAZE_MAX_CONFIG_KEY_LENGTH];
    char value[TOBIIGAZE_MAX_CONFIG_KEY_LENGTH];
};
