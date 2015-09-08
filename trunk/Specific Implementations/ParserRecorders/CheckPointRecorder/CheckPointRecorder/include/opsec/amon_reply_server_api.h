/**********************************************************
 *
 * Amon Reply API Definitions
 *
 **********************************************************/

#ifndef _AMON_REPLY_SERVER_API_H_
#define _AMON_REPLY_SERVER_API_H_

#ifndef DLLIMP
#  if (defined(WIN32) && defined(OPSEC_DLL_IMPORT))
#	      define DLLIMP __declspec( dllimport )
#  else
#	      define DLLIMP
#  endif
#endif

#include "opsec/amon_oid.h"
#include "opsec/amon_api.h"
#include "opsec/opsec_vt_api.h"

#ifdef __cplusplus
extern "C" {
#endif

/***********************************************************
 *
 * OidRep API
 *  
 ***********************************************************/
DLLIMP int 
oid_reply_create(OidRep **oid_rep);

DLLIMP void 
oid_reply_destroy(OidRep *oid_rep);

DLLIMP int 
oid_reply_set_oid(OidRep *oid_rep, 
                  const Oid *oid);

DLLIMP int 
oid_reply_set_opsec_value(OidRep *oid_rep,
                    const opsec_value_t *value);

DLLIMP void 
oid_reply_set_error(OidRep *oid_rep,
                    eOidError err);  

DLLIMP int 
oid_reply_create_with_all(OidRep **oid_rep,
                          const Oid *oid,  
                          const opsec_value_t *value,
                          eOidError err);

/***********************************************************
 *
 * AmonReply API
 *
 ***********************************************************/

DLLIMP int 
amon_reply_create(AmonReply **rep);

DLLIMP void 
amon_reply_destroy(AmonReply *rep);

DLLIMP int 
amon_reply_add_oid(AmonReply *rep,
                   const OidRep *oid_rep);

DLLIMP void 
amon_reply_set_error(AmonReply *rep, 
                     eAmonError reply_err);
DLLIMP void 
amon_reply_set_last_reply_mark(AmonReply *rep, 
                               eLastReply last_rep_mark);
DLLIMP void 
amon_reply_remove_oid(AmonReply *rep,
                      const Oid *oid);


#ifdef __cplusplus
}
#endif

#endif /* _AMON_REPLY_SERVER_API_H_ */


