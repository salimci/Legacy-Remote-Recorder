#ifndef _CVP_H_
#define _CVP_H_

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif

#define CVP_INFINITY            (1<<30)  /* the maximum chunk length */
#define	CVP_MAX_BUFFER_CHANGE	(CVP_INFINITY - 1)


DLLIMP extern OpsecEntityType *CVP_SERVER;

/*
 * Serever APIs
 */

DLLIMP int cvp_change_buffer_status(OpsecSession *session, int status, int len);
DLLIMP int cvp_send_chunk_to_dst(OpsecSession *session, char *chunk, int len);
DLLIMP int cvp_send_chunk_to_src(OpsecSession *session, char *chunk, int len);
DLLIMP int cvp_send_reply(OpsecSession *session, int opinion, char *log, OpsecInfo *info);

DLLIMP void cvp_drop(OpsecSession *session);   /* good for client as well */

DLLIMP int cvp_client_buffer_size(OpsecSession *session);
DLLIMP int cvp_max_chunk_size(OpsecSession *session, int *max);
DLLIMP int cvp_max_src_chunk_size(OpsecSession *session, int *max);
DLLIMP int cvp_cts_chunk_size(OpsecSession *session, int l);
DLLIMP int cvp_cts_src_chunk_size(OpsecSession *session, int l);

/*
 * possible return value of cvp_change_buffer_status.
 */
#define CVP_CLIENT_MAY_GET_STUCK     1 /* this is not an error, just a warning */

/*
 * possible return code for cvp_send_chunk_to_dst/src
 */
#define CVP_DATA_FLOW_DIRECTION_ERR -3
#define CVP_SEND_TO_SRC_DISABLED    -4
 
/*
 * Server hooks list
 */
#define CVP_REQUEST_HANDLER          _NEW_HANDLER_ATTR(OPSEC_CVP_SERVER, 2)
#define CVP_SERVER_CHUNK_HANDLER     _NEW_HANDLER_ATTR(OPSEC_CVP_SERVER, 1)
#define CVP_CLEAR_TO_SEND_HANDLER    _NEW_HANDLER_ATTR(OPSEC_CVP_SERVER, 3)
#define CVP_CTS_SIGNAL_HANDLER       _NEW_HANDLER_ATTR(OPSEC_CVP_SERVER, 4)

/*
 * posible status for change_status routines.
 */
#define CVP_SKIP_SRV        0x11
#define CVP_SKIP_DST        0x21
#define CVP_TRANSFER_SRV    0x12
#define CVP_TRANSFER_DST    0x22

/*
 * Possible flow directions
 */
#define UNKNOWN_FLOW 0x0
#define DST_FLOW     0x1
#define SRV_FLOW     0x2
#define SRC_FLOW     0x4

#ifdef __cplusplus
}
#endif

#endif /* _CVP_H_ */
