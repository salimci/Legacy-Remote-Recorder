/*
 * OPSEC include file for 3rd party use.
 */

#ifndef UAA_ERROR_H
#define UAA_ERROR_H

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
	UAA_REPLY_STAT_TERMINATOR                   = -1,
	UAA_REPLY_STAT_OK                           = 0,	/* must be 0 (equal to OPSEC_SESSION_OK) */
	UAA_REPLY_STAT_CLOSING_SESSION              = 1,
	UAA_REPLY_STAT_GENERAL_ERR                  = 2,
	UAA_REPLY_STAT_TIMEOUT                      = 3,
	UAA_REPLY_STAT_INVALID_REDIRECT_ADDR        = 4,
	UAA_REPLY_STAT_GET_REPLY_ERR                = 5,
	UAA_REPLY_STAT_CONNECTION_ERR               = 6,
	UAA_REPLY_STAT_SENDING_ERR                  = 7,
	UAA_REPLY_STAT_INVALID_LICENSE              = 8,
	UAA_REPLY_STAT_UNAUTH_PEER_CMD              = 9,
	UAA_REPLY_STAT_UNKNOWN_CMD                  = 10,
	UAA_REPLY_STAT_QUERY_READ_ERR               = 11,
	UAA_REPLY_STAT_INTERNAL_ERR                 = 12,
	UAA_REPLY_STAT_CMD_ABORTED                  = 13,
	UAA_REPLY_STAT_UPDATE_NOT_SUPPORTED         = 14,
	UAA_REPLY_STAT_AUTHORIZE_NOT_SUPPORTED      = 15,
	UAA_REPLY_STAT_AUTHENTICATE_NOT_SUPPORTED   = 16
} uaa_reply_status;
/* -------------------------------------------------------------------------
  |  uaa_error_str:
  |  --------------
  |  
  |  Description:
  |  ------------
  |  Provides a stringed error code for a UAA server reply.
  |
  |  Parameters:
  |  -----------
  |  status - An integer indicating the error.
  |
  |  Returned value:
  |  ---------------
  |  A string, indicating the reply error.
   ------------------------------------------------------------------------*/
DLLIMP char *uaa_error_str(uaa_reply_status status);

#ifdef __cplusplus
}
#endif
#endif /* UAA_ERROR_H */
