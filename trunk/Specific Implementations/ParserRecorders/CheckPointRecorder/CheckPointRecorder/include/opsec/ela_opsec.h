#ifndef _ELA_OPSEC_H
#define _ELA_OPSEC_H

#include "opsec/opsec.h"
#include "opsec/ela.h"

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif
	
DLLIMP extern OpsecEntityType *ELA_SERVER;
DLLIMP extern OpsecEntityType *ELA_CLIENT;

/*******************************************************
 *
 *                   C L I E N T 
 *
 *******************************************************/


DLLIMP OpsecSession *ela_new_session( OpsecEntity *client, OpsecEntity *server, Ela_CONTEXT *ctx);
DLLIMP int ela_send_log(OpsecSession *session, Ela_LOG *log);
DLLIMP void ela_end_session(OpsecSession *session);

#ifdef __cplusplus
}
#endif
#endif /* _ELA_OPSEC_H */
