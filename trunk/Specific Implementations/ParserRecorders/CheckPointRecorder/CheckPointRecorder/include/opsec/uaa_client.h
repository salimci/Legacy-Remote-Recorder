/*
 * OPSEC include file for 3rd party use.
 */

#ifndef _UAA_CLIENT_H
#define _UAA_CLIENT_H

#include "opsec/opsec.h"
#include "opsec/uaa.h"

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif
	
DLLIMP extern OpsecEntityType *UAA_SERVER;
DLLIMP extern OpsecEntityType *UAA_CLIENT;

/*
 * UAA client handlers
 */
#define UAA_QUERY_REPLY_HANDLER			_NEW_HANDLER_ATTR(OPSEC_UAA_CLIENT, 0)
#define UAA_UPDATE_REPLY_HANDLER		_NEW_HANDLER_ATTR(OPSEC_UAA_CLIENT, 1)
#define UAA_AUTHENTICATE_REPLY_HANDLER	_NEW_HANDLER_ATTR(OPSEC_UAA_CLIENT, 2)
#define UAA_AUTHORIZE_REPLY_HANDLER		_NEW_HANDLER_ATTR(OPSEC_UAA_CLIENT, 3)

/*
 * UAA client API
 */

 /* -------------------------------------------------------------------------
  |  uaa_new_session:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  Creates a new Opsec session
  |
  |  Parameters:
  |  -----------
  |  client - Pointer to the client opsec entity.
  |  server - Pointer to the server opsec entity.
  |
  |  Returned value:
  |  ---------------
  |  Pointer to the session, between the client and server entities if successful,
  |  NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP OpsecSession *uaa_new_session(OpsecEntity *client, OpsecEntity *server);

 /* -------------------------------------------------------------------------
  |  uaa_end_session:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  Ends an active Opsec session
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an Opsec session object.
  |
  |  Returned value:
  |  ---------------
  |  None.
   ------------------------------------------------------------------------*/
  DLLIMP void uaa_end_session(OpsecSession *session);

 /*
  * UAA client API
  */

 /* -------------------------------------------------------------------------
  |  uaa_send_query:
  |  ---------------
  |  
  |  Description:
  |  ------------
  |  This function sends a UAA client query to the UAA server.
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an OpsecSession object.
  |  query   - Pointer to a 'uaa_assert_t' containing the query assertions.
  |  opaque  - A general purpose opaque (this opaque is NOT sent to the
  |            server but can be accessed by the client when the reply returns).
  |  timeout - For the query (in milliseconds)
  |
  |  Returned value:
  |  ---------------
  |  Positive, non-zero cmd_id if successful. -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_send_query(OpsecSession *session,
                            uaa_assert_t *query,
                            void         *opaque,
                            unsigned int  timeout);

 /* -------------------------------------------------------------------------
  |  uaa_abort_query:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  This function aborts a UAA client query previously sent to the UAA server.
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an OpsecSession object.
  |  cmd_id  - Id of the UAA query returned from 'uaa_send_query'
  |
  |  Returned value:
  |  ---------------
  |  Zero if successful, negative value otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_abort_query(OpsecSession *session, int cmd_id);

 /* -------------------------------------------------------------------------
  |  uaa_send_update:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  This function sends a client update to the UAA server.
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an OpsecSession object.
  |  cmd_id  - Id of the UAA update.
  |  opaque  - A general purpose opaque (this opaque is NOT sent to the
  |            server but can be accessed by the client when the reply returns).
  |  timeout - For the update (in milliseconds)
  |
  |  Returned value:
  |  ---------------
  |  Positive, non-zero cmd_id if successful. -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_send_update(OpsecSession *session,
                             uaa_assert_t *update,
                             void         *opaque,
                             unsigned int  timeout);

 /* -------------------------------------------------------------------------
  |  uaa_send_authorize_request:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  This function sends a client update to the UAA server.
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an OpsecSession object.
  |  cmd_id  - Id of the UAA update.
  |  opaque  - A general purpose opaque (this opaque is NOT sent to the
  |            server but can be accessed by the client when the reply returns).
  |  timeout - For the update (in milliseconds)
  |
  |  Returned value:
  |  ---------------
  |  Positive, non-zero cmd_id if successful. -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_send_authorize_request(OpsecSession *session,
                             uaa_assert_t *auth_info,
                             void         *opaque,
                             unsigned int  timeout);

 /* -------------------------------------------------------------------------
  |  uaa_send_authenticate_request:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  This function sends a client update to the UAA server.
  |
  |  Parameters:
  |  -----------
  |  session - Pointer to an OpsecSession object.
  |  cmd_id  - Id of the UAA update.
  |  opaque  - A general purpose opaque (this opaque is NOT sent to the
  |            server but can be accessed by the client when the reply returns).
  |  timeout - For the update (in milliseconds)
  |
  |  Returned value:
  |  ---------------
  |  Positive, non-zero cmd_id if successful. -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_send_authenticate_request(OpsecSession *session,
                             uaa_assert_t *auth_info,
                             void         *opaque,
                             unsigned int  timeout);

#ifdef __cplusplus
}
#endif
#endif /* _UAA_CLIENT_H */

