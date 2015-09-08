/*
 * OPSEC include file for 3rd party use.
 */

#ifndef _OPSEC_H_
#define _OPSEC_H_

#include <sys/types.h>
#include "opsec_export_api.h"
#include "opsec_vt_api.h"
#include "stdio.h"

#ifdef __cplusplus
extern "C" {
#endif


/*
 * List of known entity type numbers.
 */
#define _NEW_EN(n, server) (((n)<<1) | server)
#define OPSEC_GENERIC    _NEW_EN(0,0)     /* should be zero */
#define OPSEC_RPC        _NEW_EN(0,1)
#define OPSEC_CVP_CLIENT _NEW_EN(1,0)    /* Each client/server pair forms a SERVICE */
#define OPSEC_CVP_SERVER _NEW_EN(1,1)
#define OPSEC_LEA_CLIENT _NEW_EN(2,0)
#define OPSEC_LEA_SERVER _NEW_EN(2,1)
#define OPSEC_SAM_CLIENT _NEW_EN(3,0)
#define OPSEC_SAM_SERVER _NEW_EN(3,1)
#define OPSEC_UFP_CLIENT _NEW_EN(4,0)
#define OPSEC_UFP_SERVER _NEW_EN(4,1)
#define OPSEC_ELA_CLIENT _NEW_EN(5,0)
#define OPSEC_ELA_SERVER _NEW_EN(5,1)
#define OPSEC_OMI_CLIENT _NEW_EN(6,0)
#define OPSEC_OMI_SERVER _NEW_EN(6,1)
#define OPSEC_CPMI_CLIENT _NEW_EN(7,0)
#define OPSEC_CPMI_SERVER _NEW_EN(7,1)
#define OPSEC_SARC_CLIENT _NEW_EN(8,0)
#define OPSEC_SARC_SERVER _NEW_EN(8,1)
#define OPSEC_CPD_CLIENT _NEW_EN(9,0)
#define OPSEC_CPD_SERVER _NEW_EN(9,1)
#define OPSEC_UAA_CLIENT _NEW_EN(10,0)
#define OPSEC_UAA_SERVER _NEW_EN(10,1)
#define OPSEC_AMON_CLIENT _NEW_EN(11,0)
#define OPSEC_AMON_SERVER _NEW_EN(11,1)
#define OPSEC_VISA_CLIENT _NEW_EN(12,0)
#define OPSEC_VISA_SERVER _NEW_EN(12,1)

#define _NEW_CONSTANT(t, etype, n)  ((t<<16)|(etype<<8)|n)
#define _NEW_DGRAM_TYPE(etype, n)   _NEW_CONSTANT(0,etype,n) /* 0 <-> MUST be short */
#define _NEW_HANDLER_ATTR(etype, n) _NEW_CONSTANT(1,etype,n)
#define _NEW_OPSEC_SIGNAL(etype, n) _NEW_CONSTANT(2,etype,n)


/*
 * PreDefinition for used structures.
 */
typedef struct _OpsecSession OpsecSession;
typedef struct _OpsecInfo OpsecInfo;
typedef struct _OpsecEnv OpsecEnv;
typedef struct _OpsecEntity OpsecEntity;
typedef struct _OpsecEntityType OpsecEntityType;

typedef struct _opsec_table *opsec_table;
typedef struct _opsec_table_iterator *opsec_table_iterator;

typedef struct _opsec_peer_addr_iter  opsec_peer_addr_iter;

/*
 * The session opaque (can be used both as a left and right value).
 */
#define SESSION_OPAQUE(session) \
			(*((void**)_opsec_get_session_opaque_ptr(session)))

/*******************************************************************
 *
 *            Attributes for  opsec_init
 *
 *******************************************************************/
#define OPSEC_EOL 				0            /* must be 0 */
#define OPSEC_CONF_FILE 		1
#define OPSEC_CONF_ARGV 		2
#define OPSEC_SIC_POLICY_FILE 	3
#define OPSEC_SIC_NAME                4 /* also for opsec_init_sic_id */
#define OPSEC_SSLCA_FILE              5 /* also for opsec_init_sic_id */
#define OPSEC_SIC_NAME		 	4
#define OPSEC_SSLCA_FILE		5
#define OPSEC_SIC_ENV           6
#define OPSEC_SHARED_LOCAL_PATH 7
#define OPSEC_SIC_PWD			8
#define OPSEC_MT				9
#define OPSEC_SIC_KEYHOLDER	10
#define OPSEC_SSLCA_BUFFER		11
#define OPSEC_SIC_ENV_BY_CONTEXT_ID	13
#define OPSEC_SIC_PRNG_SEED     14
#define OPSEC_DEBUG_FILE           15
#define OPSEC_SIC_ID_NAME         16

/*******************************************************************
 *
 *            Attributes for  opsec_init_entity
 *
 *******************************************************************/
#define OPSEC_ENTITY_NAME                   -3
#define OPSEC_SERVER_PORT                   -4 
#define OPSEC_SERVER_IP                     -5 /* for client usage */
#define OPSEC_SERVER_AUTH_PORT              -6
#define OPSEC_SERVER_AUTH_TYPE              -7
#define OPSEC_ONE_SESSION_PER_COMM          -9 /* for client usage */
#define OPSEC_ENV_T_ENV                     -10
#define OPSEC_SESSION_MULTIPLEX_MODE        -11
#define OPSEC_ENTITY_SIC_NAME               -12
#define OPSEC_ENTITY_SIC_SVC_NAME           -13
#define OPSEC_SERVER_FAILED_CONN_HANDLER    -14
#define OPSEC_SERVER_WAIT_ON_MEM_COMM_EVENT -15
#define OPSEC_SERVER_CTX_DECISION_STATE	-16
#define OPSEC_SERVER_DIRECTION                     -17
#define OPSEC_SERVER_CONN_BUF_SIZE             -18
#define OPSEC_SERVER_NO_NAGLE                      -19
#define OPSEC_SERVER_QUEUE_SIZE_LIMIT       -100

/*******************************************************************
 *
 *            Sub Attributes for OPSEC_SESSION_MULTIPLEX_MODE
 *
 *******************************************************************/
#define MULT_ALL_ON_ONE		-1
#define MULT_DYNAMIC		 0
#define MULT_ONE_PER_COMM	 1


/*******************************************************************
 *
 *            Sub Attributes for OPSEC_SERVER_DIRECTION
 *
 *******************************************************************/

typedef enum { SERVER_DIRECTION_ANY=0, 
                          SERVER_DIRECTION_INBOUND=1,
                          SERVER_DIRECTION_OUTBOUND=2} OpsecServerDirection;

#define OPSEC_SESSION_START_HANDLER   		_NEW_HANDLER_ATTR(OPSEC_GENERIC, 6)
#define OPSEC_SESSION_END_HANDLER     		_NEW_HANDLER_ATTR(OPSEC_GENERIC, 7)
#define OPSEC_SIGNALS_HANDLER         		_NEW_HANDLER_ATTR(OPSEC_GENERIC, 8)
#define OPSEC_SESSION_ESTABLISHED_HANDLER 	_NEW_HANDLER_ATTR(OPSEC_GENERIC, 9)
#define OPSEC_GENERIC_SESSION_START_HANDLER	_NEW_HANDLER_ATTR(OPSEC_GENERIC, 10)
#define OPSEC_GENERIC_SESSION_END_HANDLER  	_NEW_HANDLER_ATTR(OPSEC_GENERIC, 11)
#define OPSEC_SERVER_CTX_DECISION_HANDLER   _NEW_HANDLER_ATTR(OPSEC_GENERIC, 12)

/***************************************************
 * Possible return codes from handlers
 * This are the return codes that the entity
 * demultiplexer should return.
 ***************************************************/
typedef enum {
	OPSEC_SESSION_OK = 0,   /* must be 0 */
	OPSEC_SESSION_END = 1,
	OPSEC_SESSION_ERR = -1,
	OPSEC_BAD_DGTYPE = -2  /* In case of unknown dgtype.  */
} eOpsecHandlerRC;

/******************************************
 * Possible authentication methods/encryption
 ******************************************/
#define OPSEC_AUTH_FWN1             1
#define OPSEC_AUTH_SSL              2
#define OPSEC_AUTH_FWN1_AND_SSL     3
#define OPSEC_AUTH_SSL_CLEAR        4
#define OPSEC_NONE                  5
#define OPSEC_FWN1                  6
#define OPSEC_SSL                   7
#define OPSEC_SSLCA                 8
#define OPSEC_ASYM_SSLCA            9
#define OPSEC_LOCAL                 10
#define OPSEC_SSLCA_COMP            11
#define OPSEC_SSLCA_RC4             12
#define OPSEC_SSLCA_RC4_COMP        13
#define OPSEC_ASYM_SSLCA_COMP       14
#define OPSEC_ASYM_SSLCA_RC4        15
#define OPSEC_ASYM_SSLCA_RC4_COMP   16
#define OPSEC_SSL_CLEAR             17
#define OPSEC_SSLCA_CLEAR           18

/***********************************
 * Possible communication types.
 * Comm type is part of the address.
 ***********************************/
#define OPSEC_TCP_COMM  1
#define OPSEC_MEM_COMM  2   /* peer in the same process       */
#define OPSEC_NON_COMM  3   /* no peer. no real communication */
#define OPSEC_AUTH_COMM 4   /* authenticated tcp/ip communication */


/******************************************************************
 *
 *      F u n c t i o n s   P r o t o t y p e
 *
 ******************************************************************/

/*
 * For OPSEC USERS
 */




DLLIMP OpsecEnv	*opsec_init(int, ...);
DLLIMP void opsec_env_destroy(OpsecEnv *);
DLLIMP int opsec_init_sic_id(OpsecEnv* env,int,...);
DLLIMP int opsec_destroy_sic_id (OpsecEnv* env,int,...);
DLLIMP OpsecEntity	*opsec_init_entity(OpsecEnv *, OpsecEntityType *, ...);
DLLIMP int	opsec_mainloop(OpsecEnv *);
DLLIMP void	opsec_destroy_entity(OpsecEntity *);
DLLIMP int	opsec_start_server(OpsecEntity *);
DLLIMP int	opsec_stop_server(OpsecEntity *);
DLLIMP void	opsec_end_session(OpsecSession *session);
DLLIMP int  opsec_session_is_closed(OpsecSession *session);
DLLIMP char	*opsec_get_conf(OpsecEnv *, ...);
DLLIMP char	*opsec_info_get(OpsecInfo *, ...);
DLLIMP int	opsec_info_set(OpsecInfo *, ...);
DLLIMP void	opsec_info_destroy(OpsecInfo *);
DLLIMP OpsecInfo	*opsec_info_init();
DLLIMP OpsecSession *opsec_new_generic_session(OpsecEntity *, OpsecEntity *);
DLLIMP int opsec_start_keep_alive(OpsecSession *, int);
DLLIMP int opsec_stop_keep_alive(OpsecSession *);
DLLIMP int opsec_get_queue_state(OpsecSession *session);

DLLIMP int opsec_ping_peer(OpsecSession *session, int timeout, 
						   void (*handler)(OpsecSession*, 
						   				   unsigned int, 	/* Opsec Info bitmask */
						   				   OpsecInfo*, 		
						   				   time_t, 			/* Round trip time mSec */
						   				   int, 			/* Status */
						   				   void*),			/* Callback Opaque */
						   void*  opaque);


/******************
  * APIs for Multi-SIC 
  ******************/

#define OPSEC_CTX_ACCEPT  1
#define OPSEC_CTX_CLIENT_SIC_NAME  2 
#define OPSEC_CTX_SERVER_SIC_NAME  3


typedef struct _OpsecConnInfo OpsecConnInfo;

DLLIMP int opsec_conn_info_get_fd (const OpsecConnInfo *info); /* returns -1 on failure */

DLLIMP const char* opsec_conn_info_get_client_sic_name(const OpsecConnInfo *info); /* sic name should be duplicated for further use */
DLLIMP const char *opsec_conn_info_get_server_sic_name(const OpsecConnInfo *info);




#define PING_PEER_STAT_OK		 0
#define PING_PEER_STAT_SEND_ERR	-3
#define PING_PEER_STAT_TIMEOUT	-5	


DLLIMP OpsecEnv	*opsec_get_session_env(OpsecSession *);


DLLIMP int opsec_session_end_reason(OpsecSession *session);
#define SESSION_NOT_ENDED               0
#define END_BY_APPLICATION              1
#define UNABLE_TO_ATTACH_COMM           2
#define ENTITY_TYPE_SESSION_INIT_FAIL   3
#define ENTITY_SESSION_INIT_FAIL        4
#define COMM_FAILURE                    5
#define BAD_VERSION                     6
#define PEER_SEND_DROP                  7
#define PEER_ENDED                      8
#define PEER_SEND_RESET                 9
#define COMM_IS_DEAD                   10
#define	SIC_FAILURE					   11
#define SESSION_TIMEOUT                12

DLLIMP int opsec_get_sic_error(OpsecSession *session, int *sic_errno, char **sic_errmsg);
DLLIMP char * opsec_sic_get_peer_sic_name(OpsecSession *session);
DLLIMP char * opsec_sic_get_sic_method(OpsecSession *session);
DLLIMP int opsec_sic_get_peer_cert_hash(OpsecSession *session, unsigned char *hash, unsigned int *hash_len,
                                        char *cert_str, int cert_str_len);
DLLIMP char * opsec_sic_get_sic_service(OpsecSession *session, short *service_num);

DLLIMP int opsec_add_sic_rule(OpsecEnv *env, int direction, char *apply_to, char *peer, char *dst_port, 
                              char *svc_name, char *method_name, void **rule);

DLLIMP int opsec_delete_sic_rule(OpsecEnv *env, void *rule);
DLLIMP char *opsec_get_my_sic_name(OpsecEnv *env);

DLLIMP void	opsec_schedule(OpsecEnv *,time_t, void(*)(void*), void*);
DLLIMP void	opsec_periodic_schedule(OpsecEnv *,time_t, void(*)(void*), void*);
DLLIMP void	opsec_deschedule(OpsecEnv *,void(*)(void*), void*);
DLLIMP void	opsec_set_socket_event(OpsecEnv *env, int event, int sock,
				int(*)(int, void*), void*);
DLLIMP void	opsec_del_socket_event(OpsecEnv *env, int event, int sock);

DLLIMP void opsec_get_sdk_version(int *sdk_ver, int *pn, int *bn, char **vd, char **fd);
DLLIMP int opsec_get_peer_sdk_version(OpsecSession *session, void (*f)(OpsecSession*,int,int,int,char*,char*));

DLLIMP void opsec_resume_session_read(OpsecSession *session);
DLLIMP void opsec_suspend_session_read(OpsecSession *session);

DLLIMP void opsec_free(void *p);

DLLIMP void opsec_set_debug_level(int level);
DLLIMP void opsec_set_debug_file(FILE* file);

/*
 * Possible events to set/del socket_event.
 */
#define OPSEC_SK_INPUT       1
#define OPSEC_SK_OUTPUT      2
#define OPSEC_SK_EXCEPTIONAL 3


/*
 * This routine should be used through the OPAQUE macro:
 * 	SESSION_OPAQUE(session)
 */
DLLIMP void** _opsec_get_session_opaque_ptr(OpsecSession *);

/*
 * Quering what is local/peer's address.
 * If comm type is TCP or AUTH then ip and port may contain information
 * (in network order). If both are zero, then we do no have, yet, the information
 * (connection is not connected, yet).
 */
DLLIMP int opsec_get_local_address(OpsecSession *session,int *type, unsigned int *ip, unsigned short *port);
DLLIMP int opsec_get_peer_address(OpsecSession *session,int *type, unsigned int *ip, unsigned short *port);

DLLIMP char * opsec_get_peer_auth_name(OpsecSession *session);

DLLIMP opsec_peer_addr_iter *
            opsec_create_peer_addr_iter(OpsecSession *session);
DLLIMP void opsec_destroy_peer_addr_iter(opsec_peer_addr_iter *iter);
DLLIMP unsigned int
            opsec_peer_addr_iter_get_next(opsec_peer_addr_iter *iter);

DLLIMP OpsecEntity * opsec_get_own_entity(OpsecSession *session);
DLLIMP OpsecEntity * opsec_get_peer_entity(OpsecSession *session);

DLLIMP char * opsec_get_entity_name(OpsecEntity *ent);

DLLIMP int    opsec_get_entity_id(OpsecEntity *ent);

DLLIMP int opsec_set_session_timeout(OpsecSession *session, int timeout);
DLLIMP int opsec_set_session_timeout_handler(OpsecSession *session, int(*)(OpsecSession *));
DLLIMP int opsec_session_get_my_sic_name(OpsecSession* session, char ** sic_name); 
DLLIMP int opsec_session_signal_handler_enable(OpsecSession *session);
DLLIMP int opsec_session_signal_handler_disable(OpsecSession *session);

#ifdef WIN32
#include <windows.h>

BOOL WINAPI
opsec_DllMain(HINSTANCE hinstDLL,  /* handle to DLL module */
              DWORD fdwReason,     /* reason for calling function */
              LPVOID lpReserved ); /* reserved */

#endif /* WIN32 */



#ifdef __cplusplus
}
#endif

#endif /* _OPSEC_H_ */
