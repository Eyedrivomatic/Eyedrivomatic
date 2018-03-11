#include "tobiigaze_data_types.h"
#include "tobiigaze_error_codes.h"
#include "tobiigaze_callback_types.h"
#include "tobiigaze_define.h"


#ifdef __cplusplus
extern "C" {
#endif
    /**
     * Collects information about all usb eye trackers currently connected to the machine.
     * @param device_infos          A pre-allocated array that will contain device information about the eye trackers.
     * @param device_infos_cap      The capacity of the device_infos array.
     * @param device_infos_size     The number of eye trackers found.
     * @param error_code            Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
     */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_list_usb_eye_trackers(struct usb_device_info *device_infos, uint32_t device_infos_cap, uint32_t *device_infos_size, tobiigaze_error_code *error_code);

    /**
    * Gets the url to a connected eye tracker. If mutliple eye trackers are connected, an arbitary eye tracker will be returned.
    * @param url          Buffer where the url will be written to.
    * @param url_size     Size of the buffer in bytes.
    * @param error_code   Will be set to TOBIIGAZE_ERROR_SUCCESS if operation was successful, otherwise to an error code. Can be NULL.
    */
    TOBIIGAZE_API void TOBIIGAZE_CALL tobiigaze_get_connected_eye_tracker(char *url, uint32_t url_size, tobiigaze_error_code *error_code);

#ifdef __cplusplus
}
#endif
