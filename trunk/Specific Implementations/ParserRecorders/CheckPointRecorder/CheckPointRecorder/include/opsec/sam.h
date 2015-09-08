#ifndef _SAM_H
#define _SAM_H
#endif

#include "opsec.h"
#include "sam_codes.h"

#ifndef _SAMOPSECCLIENT_H
#define _SAMOPSECCLIENT_H

#include "opsec_export_api.h"
 
#ifdef __cplusplus
extern "C" {
#endif


#define SAM_SERVER_PORT 18183

#define SAM_ACK_HANDLER	_NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 1)
#define SAM_CLOSED_HANDLER	_NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 2)
#define SAM_FLOW_HANDLER	_NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 3)
#define SAM_INFO_ACK_HANDLER    _NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 4)
#define SAM_MONITOR_ACK_HANDLER    _NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 4)
#define SAM_MONITOR_ACK_WITH_INFO_HANDLER   _NEW_HANDLER_ATTR(OPSEC_SAM_CLIENT, 5)

#define _SAM_INHIBIT_FLAG           0x0001
#define _SAM_CLOSE_FLAG             0x0002
#define _SAM_NOTIFY_FLAG            0x0004
#define _SAM_CANCEL_FLAG            0x0008
#define _SAM_UNINHIBIT_FLAG         0x0010
#define _SAM_UNINHIBIT_ALL_FLAG     0x0020
#define _SAM_DELETE_ALL_FLAG        0x0040
#define _SAM_RETRIEVE_INFO_FLAG     0x80
#define _SAM_MONITOR_FLAG           0x80
#define _SAM_DROP_FLAG              0x100
#define _SAM_REJECT_FLAG            0x200
#define _SAM_BYPASS_FLAG            0x400
#define _SAM_QUARANTINE_FLAG        0x800



/* 
 *	Action Types
 */

#define SAM_INHIBIT	_SAM_INHIBIT_FLAG
#define SAM_INHIBIT_AND_CLOSE	(_SAM_INHIBIT_FLAG | _SAM_CLOSE_FLAG)
#define	SAM_CLOSE	_SAM_CLOSE_FLAG
#define SAM_NOTIFY	_SAM_NOTIFY_FLAG
#define	SAM_UNINHIBIT	_SAM_UNINHIBIT_FLAG
#define SAM_UNINHIBIT_ALL _SAM_UNINHIBIT_ALL_FLAG
#define SAM_DELETE_ALL _SAM_DELETE_ALL_FLAG
#define SAM_CANCEL	_SAM_CANCEL_FLAG
#define SAM_BYPASS                _SAM_BYPASS_FLAG
#define SAM_QUARANTINE       _SAM_QUARANTINE_FLAG

#define SAM_DROP			_SAM_DROP_FLAG
#define SAM_REJECT			_SAM_REJECT_FLAG
#define SAM_INHIBIT_DROP		(SAM_INHIBIT | SAM_DROP)
#define SAM_INHIBIT_DROP_AND_CLOSE	(SAM_INHIBIT_AND_CLOSE | SAM_DROP)

#define SAM_RETRIEVE_INFO  		_SAM_RETRIEVE_INFO_FLAG
#define SAM_MONITOR  			_SAM_MONITOR_FLAG


/* 
 * Filter Types
 *
 * The idea is to use a bit-mask model
 */
#define SAM_ANY_IP		0x1
#define SAM_SRC_IP		0x2
#define SAM_DST_IP		0x4

#define SAM_SERV_SRC_IP 	0x8	/* these two are used at the server side 		*/ 
#define SAM_SERV_DST_IP 	0x10	/* to set IP flag in kernel sam_blocked_ips tables	*/

#define SAM_SERV_OLD		0x20

/*
 * The new ones 
 */

#define SAM_SPORT		0x40		
#define SAM_DPORT		0x80		
#define SAM_PROTO		0x100
#define SAM_SMASK		0x200
#define SAM_DMASK		0x400

#define SAM_ALL			0x800				/* for monitoring */

#define SAM_SRC_SERV 		(SAM_SRC_IP              | SAM_DPORT | SAM_PROTO)
#define SAM_DST_SERV 		(             SAM_DST_IP | SAM_DPORT | SAM_PROTO)
#define SAM_SRC_IP_PROTO 	(SAM_SRC_IP              |             SAM_PROTO)
#define SAM_DST_IP_PROTO	(             SAM_DST_IP |             SAM_PROTO)
#define SAM_SERV		(SAM_SRC_IP | SAM_DST_IP | SAM_DPORT | SAM_PROTO)

#define SAM_SUB_ANY_IP		(SAM_ANY_IP | SAM_SMASK | SAM_DMASK)

#define SAM_SUB_SRC_IP		(SAM_SRC_IP | SAM_SMASK)
#define SAM_SUB_DST_IP		(SAM_DST_IP | SAM_DMASK)
#define SAM_SUB_SRC_SERV	(SAM_SRC_SERV | SAM_SMASK)
#define SAM_SUB_DST_SERV	(SAM_DST_SERV | SAM_DMASK)
#define SAM_SUB_SRC_IP_PROTO	(SAM_SRC_IP_PROTO | SAM_SMASK)
#define SAM_SUB_DST_IP_PROTO	(SAM_DST_IP_PROTO | SAM_DMASK)

#define SAM_SUB_SERV		(SAM_SERV | SAM_SMASK | SAM_DMASK)
#define SAM_SUB_SERV_SRC  	(SAM_SERV | SAM_SMASK)
#define SAM_SUB_SERV_DST	(SAM_SERV | SAM_DMASK)

#define SAM_DUMMY_FILTER	0x00
#define SAM_WILD_CARD		0x8000 
#define SAM_GENERIC_FILTER	(SAM_WILD_CARD | SAM_SUB_SERV)
#define SAM_GENERIC_REQUEST	0x1000
/* 
 * Log Types
 */

#define SAM_NOLOG			0
#define SAM_SHORT_NOALERT	1
#define SAM_SHORT_ALERT		2
#define SAM_LONG_NOALERT	3
#define SAM_LONG_ALERT		4

/*
 *	Timeout Types
 */
#define	SAM_EXPIRE_NEVER	0


/* argument types */
#define SAM_EXPIRE          1
#define SAM_REQ_TYPE        2
#define SAM_RULE_INFO       3


DLLIMP extern OpsecSession *sam_new_session(OpsecEntity *client, OpsecEntity *server);



DLLIMP extern int 
sam_client_action(	OpsecSession *session, int actions, 
					int log_flag, char *fwhost, void *request_id,  ... );



DLLIMP extern int sam_table_get_nrows(opsec_table tab); 

DLLIMP extern int sam_table_get_ncols(opsec_table tab);

DLLIMP extern opsec_vtype *sam_table_get_format(opsec_table tab);

DLLIMP extern opsec_table_iterator sam_table_iterator_create(opsec_table tab);

DLLIMP extern void sam_table_iterator_destroy(opsec_table_iterator iter);

DLLIMP extern void *sam_table_iterator_next(opsec_table_iterator iter, opsec_vtype *vtype);

DLLIMP extern int sam_retrieve_info( OpsecSession*, int , char*, void*, ...);
DLLIMP extern int sam_client_monitor( OpsecSession*, int , char*, void*, ...);

DLLIMP extern OpsecEntityType *SAM_CLIENT;
DLLIMP extern OpsecEntityType *SAM_SERVER;    /* used by OPSEC SAM Clients */
DLLIMP extern OpsecEntityType *SAM_SERVER_FW; /* used by FW-1 SAM Server   */
DLLIMP extern char * sam_error_str(int status);

/* 
 *  SamVarList APIs 
 */

typedef struct _SamVarList      SamVarList;
typedef struct _SamVarListIter  SamVarListIter;

#define SAM_VAR_FOUND       1
#define SAM_VAR_NOT_FOUND   0
#define SAM_VAR_ERR        -1

DLLIMP SamVarList*     SamVarListCreate();
DLLIMP void            SamVarListDestroy(SamVarList * list);
DLLIMP int             SamVarListAdd(SamVarList * list, const char * name, const char * value);
DLLIMP int             SamVarListGetVal(SamVarList * list, const char * name, char ** val);
DLLIMP SamVarListIter* SamVarListIterCreate(SamVarList * list);
DLLIMP void            SamVarListIterDestroy(SamVarListIter *iter);
DLLIMP int             SamVarListIterGetNext(SamVarListIter * iter, char **name, char **val);

#define SAM_DEFAULT_COLS    9

#ifdef __cplusplus
}
#endif

#endif /* _SAM_H */
