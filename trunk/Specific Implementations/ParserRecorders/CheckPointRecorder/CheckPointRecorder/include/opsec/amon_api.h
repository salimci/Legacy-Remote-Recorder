/**********************************************************
 *
 * Amon Shared API Definitions
 *
 **********************************************************/

#ifndef _AMON_API_H_
#define _AMON_API_H_

#ifndef DLLIMP
#  if (defined(WIN32) && defined(OPSEC_DLL_IMPORT))
#	      define DLLIMP __declspec( dllimport )
#  else
#	      define DLLIMP
#  endif
#endif

#include "opsec/opsec_vt_api.h"
#include "opsec/amon_oid.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * id of request
 */
typedef int AmonReqId;

 
/*
 * OID Error Codes
 */
typedef enum {
	OidErr_Ok       = 0,
	OidErr_NotFound = -1,
	OidErr_Error    = -2,
	OidErr_ReadOnly = -3
} eOidError;


/*
 * Possible Error codes on reply 
 */
typedef enum {
    AmonError_OK      =  0,
    AmonError_Fail    = -1,
    AmonError_Error   = -2
} eAmonError;


/*
 * Last Reply Marker
 */
typedef enum {
    LastReply_False = 0,
    LastReply_True,
    LastReply_Error,
    LastReply_LastChunk
} eLastReply;


/*
 * Reply Modes
 */
typedef enum {
    AmonRepMode_All,
    AmonRepMode_Partial,
    AmonRepMode_None
} eAmonRepMode;

/*
 * Search Scope
 */
typedef enum {      
    AmonScope_GetTableAndUpdate = -1,       /* table search */
    AmonScope_GetAll,                       /* this oid and all its sub tree oid's */
    AmonScope_GetOne,                       /* one oid only */
    AmonScope_GetNext,                      /* next oid only */
    AmonScope_Error,
    AmonScope_SetOne
} eAmonScope;

typedef enum {
	AmonNotify_False = 0,
	AmonNotify_True
} eAmonNotify;


typedef enum {
    AmonNotifyReplyMode_Full,
    AmonNotifyReplyMode_Update
} eAmonNotifyReplyMode;


/************************************************************
 *
 * Functions Prototypes
 *
 ************************************************************/        
                      
/***********************************************************
 *
 * AmonRequest API
 *
 ***********************************************************/
typedef struct _AmonRequest   AmonRequest;

DLLIMP unsigned int 
amon_request_get_num_of_oids(const AmonRequest *req);

DLLIMP eAmonRepMode
amon_request_get_reply_mode(const AmonRequest *req);

DLLIMP unsigned int 
amon_request_get_size_limit(const AmonRequest *req);

DLLIMP eAmonScope 
amon_request_get_scope(const AmonRequest *req);

DLLIMP eAmonNotify 
amon_request_notify_get(AmonRequest *req, 
                        unsigned int *polling_interval,
                        unsigned int *period_interval);
                              
DLLIMP eAmonNotifyReplyMode 
amon_request_notify_get_mode(AmonRequest *req);

DLLIMP const char *
amon_request_get_peer(const AmonRequest *req);

DLLIMP const char * 
amon_request_get_msg(const AmonRequest *req);

/***********************************************************
 *
 * AmonRequestIter API
 *
 ***********************************************************/
typedef struct _AmonRequestIter   AmonRequestIter;

DLLIMP int 
amon_request_iter_create(AmonRequest *req, 
                         AmonRequestIter **iter);

DLLIMP const Oid *
amon_request_iter_next(AmonRequestIter *iter); 

DLLIMP const Oid * 
amon_request_iter_next_for_set(AmonRequestIter *iter,		
                       		   const opsec_value_t **value);

DLLIMP void 
amon_request_iter_destroy(AmonRequestIter *iter);


/***********************************************************
 *
 * OidRep API
 *
 ***********************************************************/
typedef struct _OidRep     OidRep;

DLLIMP const Oid * 
oid_reply_get_oid(const OidRep *oid_rep);

DLLIMP const opsec_value_t * 
oid_reply_get_opsec_value(const OidRep *oid_rep);

DLLIMP eOidError 
oid_reply_get_error(const OidRep *oid_rep);  

DLLIMP void
oid_reply_get_all(const OidRep *oid_rep,
                  const Oid **oid, 
                  const opsec_value_t **value,
                  eOidError *err);                          


/***********************************************************
 *
 * AmonReply API
 *
 ***********************************************************/
typedef struct _AmonReply   AmonReply;

DLLIMP unsigned int 
amon_reply_get_num_of_oids(const AmonReply *rep);

DLLIMP eAmonError 
amon_reply_get_error(const AmonReply *rep);

DLLIMP eLastReply 
amon_reply_get_last_reply_mark(const AmonReply *rep);

/***********************************************************
 *
 * AmonReplyIter API
 *
 ***********************************************************/

typedef struct _AmonReplyIter  AmonReplyIter;

DLLIMP int 
amon_reply_iter_create(AmonReply *rep, 
                         AmonReplyIter **iter);
DLLIMP const OidRep * 
amon_reply_iter_next(AmonReplyIter *iter); 

DLLIMP void 
amon_reply_iter_destroy(AmonReplyIter *iter);


#ifdef __cplusplus
}
#endif

#endif /* _AMON_API_H_ */

