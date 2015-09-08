/**********************************************************
 *
 * Amon Server API Definitions
 *
 **********************************************************/
#ifndef _AMON_SERVER_H_
#define _AMON_SERVER_H_

#ifndef DLLIMP
#  if (defined(WIN32) && defined(OPSEC_DLL_IMPORT))
#	      define DLLIMP __declspec( dllimport )
#  else
#	      define DLLIMP
#  endif
#endif

#include "opsec/opsec.h"
#include "opsec/amon_api.h"

#ifdef __cplusplus
extern "C" {
#endif

/**********************************************
 *
 * Entity Types 
 *
 **********************************************/
DLLIMP extern OpsecEntityType *AMON_CLIENT;
DLLIMP extern OpsecEntityType *AMON_SERVER;

/**********************************************
 *
 * Handlers Types 
 *
 **********************************************/
 #define AMON_REQUEST_HANDLER         _NEW_HANDLER_ATTR(OPSEC_AMON_SERVER, 0)
 #define AMON_CANCEL_HANDLER          _NEW_HANDLER_ATTR(OPSEC_AMON_SERVER, 1)

/**********************************************
 *
 * Handlers Prototypes 
 *
 **********************************************/
typedef eOpsecHandlerRC (stat_request_handler)(OpsecSession *session, 
                                               AmonRequest *req,
                                               AmonReqId id);

typedef eOpsecHandlerRC (stat_cancel_handler)(OpsecSession *session, 
                                              AmonReqId id);                                                

/**********************************************
 *
 * Functions Prototypes 
 *
 **********************************************/
/*
 * sends the reply on the session
 * params:
 *  session -
 *  rep - the reply to send
 *  id  - the id of the request that this reply answers
 *
 * return EO_OK on success, else EO_ERROR
 */
DLLIMP int 
amon_reply_send(OpsecSession *session, 
                AmonReply *rep,
                AmonReqId id);


#ifdef __cplusplus
}
#endif

#endif /* _AMON_SERVER_H_ */

