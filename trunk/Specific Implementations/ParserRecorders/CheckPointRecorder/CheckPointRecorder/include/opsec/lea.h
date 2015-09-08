
/*
 * LEA include file for 3rd party use
 */

#ifndef	_h_lea_
#define	_h_lea_

#include <time.h>
#include "opsec_export_api.h"
#include "opsec.h"
#include "opsec/opsec_uuid.h"

#ifdef	__cplusplus
extern "C" {
#endif

/* ----------------------------------------------------------------------
 * Possible attributes for LEA_VT 
 * ----------------------------------------------------------------------*/
typedef int LEA_VT;

#define LEA_VT_NONE			  0
#define	LEA_VT_ACTION		  1	/* accept, drop, deauthorize, etc.     */
#define	LEA_VT_INTERFACE	  2	/* le0, daemon...                      */
#define	LEA_VT_ALERT		  3	/* mail, snmp_trap ...                 */
#define	LEA_VT_RULE		      4	/* old, internal, 1, 2, ..             */
#define LEA_VT_INT            5
#define	LEA_VT_DIRECTION      6	 /* inbound, outbound                   */
#define	LEA_VT_IP_PROTO	      7	 /* TCP, UDP, ICMP                      */
#define	LEA_VT_IP_ADDR	      8
#define LEA_VT_RPC_PROG       9
#define	LEA_VT_TCP_PORT	     10
#define	LEA_VT_UDP_PORT	     11
#define	LEA_VT_HEX           12 
#define	LEA_VT_TIME	         13
#define	LEA_VT_STRING	     14
#define	LEA_VT_MASK	         15
#define LEA_VT_DURATION_TIME 16 /* the 'elapsed' time (as in the acconting log file.                 */
#define LEA_VT_USHORT        17 /* for s_port or d_port other than TCP_PORT OR UDP_PORT                */
#define LEA_VT_SR_HOSTNAME   18
#define LEA_VT_SR_HOSTGROUP  19
#define LEA_VT_SR_USERGROUP  20
#define LEA_VT_SR_SERVICE    21
#define LEA_VT_SR_SERVICEGROUP 22
#define LEA_VT_ISTRING       23
#define LEA_VT_UUID			  24
#define LEA_VT_IPV6  25

/*
 * The last virtual type
 */
#define LEA_VT_LAST			LEA_VT_IPV6+1

typedef union _lea_value{
	char *string_value;
	int i_value;
	unsigned short ush_value;
	unsigned char uch_value;
	unsigned int ul_value;
	opsec_uuid *uuid_value;
	opsec_in6_addr ipv6addr_value;
} lea_value_t;


typedef struct _lea_value_ex_t lea_value_ex_t;

typedef struct lea_field_type {
	int lea_attr_id;	/* The index of the matching format function */
	LEA_VT lea_val_type;
	lea_value_t lea_value;
	int lea_dictionary;	/* The id of the matching dictionary */
} lea_field;

typedef struct lea_record_type {
	int n_fields;
	lea_field *fields;
} lea_record;

#define	lea_string	lea_value.string_value
#define	lea_int	lea_value.i_value
#define	lea_u_short	lea_value.ush_value
#define	lea_u_char	lea_value.uch_value
#define	lea_u_long	lea_value.ul_value


/* ----------------------------------------------------------------------
 * 'defines' according to LEA_VT, to simplify the access to lea_value
 * ----------------------------------------------------------------------*/
#define	lea_action	lea_value.i_value
#define	lea_interface	lea_value.i_value
#define	lea_alert	lea_value.i_value
#define	lea_direction	lea_value.uch_value
#define	lea_ip_proto	lea_value.uch_value
#define	lea_ip_addr	lea_value.ul_value
#define	lea_tcp_port	lea_value.ush_value
#define	lea_udp_port	lea_value.ush_value
#define	lea_rpc_prog	lea_value.ul_value
#define	lea_hex	lea_value.ul_value
#define	lea_time	lea_value.ul_value
#define	lea_rule	lea_value.i_value
#define	lea_duration_time	lea_value.ul_value
#define	lea_mask	lea_value.ul_value
#define	lea_uuid	lea_value.uuid_value
#define	lea_ipv6addr	lea_value.ipv6addr_value

/* ---------------------------------------
 * LEA Extended Value APIs
 * ---------------------------------------*/
DLLIMP lea_value_ex_t *
lea_value_ex_create();

DLLIMP void
lea_value_ex_destroy(lea_value_ex_t *val);

DLLIMP lea_value_ex_t *
lea_value_ex_duplicate(lea_value_ex_t *val);

DLLIMP int
lea_value_ex_get(lea_value_ex_t *val, ...);

DLLIMP int
lea_value_ex_set(lea_value_ex_t *val, LEA_VT type, ...);

DLLIMP int
lea_value_ex_set_type(lea_value_ex_t *val, LEA_VT type);

DLLIMP int
lea_value_ex_get_type(lea_value_ex_t *val, LEA_VT *type);


typedef struct lea_dict_entry_type{
	char *lea_d_name;
	lea_value_t  lea_d_value;
} lea_dict_entry;

#define	lea_d_int	    lea_d_value.i_value
#define	lea_d_u_long	lea_d_value.ul_value
#define	lea_d_u_short	lea_d_value.ush_value
#define lea_d_u_char    lea_d_value.uch_value
/* ----------------------------------------------------------------------
 * 'defines' according to LEA_VT, to simplify the access to lea_d_value
 * ----------------------------------------------------------------------*/
#define lea_d_attrib        lea_d_value.i_value
#define	lea_d_action	    lea_d_value.i_value
#define	lea_d_interface	    lea_d_value.i_value
#define	lea_d_alert	        lea_d_value.i_value
#define	lea_d_direction	    lea_d_value.uch_value
#define	lea_d_ip_proto	    lea_d_value.uch_value
#define	lea_d_ip_addr	    lea_d_value.ul_value
#define	lea_d_tcp_port	    lea_d_value.ush_value
#define	lea_d_udp_port	    lea_d_value.ush_value
#define	lea_d_rpc_prog	    lea_d_value.ul_value
#define	lea_d_hex	        lea_d_value.ul_value
#define	lea_d_time	        lea_d_value.ul_value
#define	lea_d_rule	        lea_d_value.i_value
#define	lea_d_duration_time	lea_d_value.ul_value
#define	lea_d_mask	        lea_d_value.ul_value


typedef struct lea_log_track_desc{
  int fileid;
  char *filename;
} lea_logdesc;

/* ----------------------------------------------------------------------
 * Fixed dictionary IDs (This dictionaries are sent in every session).
 * ----------------------------------------------------------------------*/

/*
 * The possible values in this dictionary:
 * "time", "orig", "action", "src", "dst", etc.
 */
#define	LEA_ATTRIB_ID	0

/*
 * This dictionary holds the ip-addresses in your
 * database ( described in 'Network Objects')
 * The ip-addresses are in their unsigned long format.
 */
#define	LEA_IP_ID	1

/*
 * This dictionary holds the possible actions:
 * "accept", "drop", "reject", "encrypt" etc.
 */
#define	LEA_ACTION_ID	2

/*
 * This dictionary holds the UDP services you
 * have in your database (are described in your
 * 'Services Manager').
 */
#define	LEA_UDP_SERVICES_ID	3

/*
 * This dictionary holds the TCP services you
 * have in your database (are described in your
 * 'Services Manager').
 */
#define	LEA_TCP_SERVICES_ID	4


/*
 * For each field that does not request a dictionary, its lea_dictionary field
 * will equal LEA_NO_DICT.
 */
#define LEA_NO_DICT        -1
struct lea_dict_iter_type;
typedef struct lea_dict_iter_type lea_dict_iter;


/*************************************************************
 *
 *                      CLIENT API
 *
 *************************************************************/

/*
 * Return value: the formatted field .
 */
DLLIMP char *
lea_resolve_field(OpsecSession *session, lea_field field);

/*
 * Return value: if found in dictionary: the field converted to string
 * otherwise NULL.
 */
DLLIMP char *
lea_dictionary_lookup (OpsecSession *session, lea_value_t value, int dict_id);

/*
 * Return value: if the value was found, returns in the given lea_value_t
 * union the matching lea_d_value, and returns FOUND, else returns NOT_FOUND
 */
DLLIMP int
lea_reverse_dictionary_lookup(OpsecSession *session, int dict_id, char *value,
		lea_value_t *lea_d_value);
/*
 * Possible return value from lea_dictionary_lookup,
 * lea_reverse_dictionary_lookup...
 */
#define	LEA_NOT_FOUND	-1
#define	LEA_FOUND	1

/*
 * Return value: the correct attribute field (the matching log-column).
 */
DLLIMP char *
lea_attr_name(OpsecSession *session, int lea_attr_id);

/*
 * Return value: if the relevant dict == NULL, returns NULL, otherwise returns
 * an iterator.
 */
DLLIMP lea_dict_iter *
lea_dict_iter_create(OpsecSession *session, int dict_id, int n_entry);

/*
 * Call this function, every time you finished working with an iterator.
 */
DLLIMP void
lea_dict_iter_destroy(lea_dict_iter *iter);

/*
 * Returns the next entry in the dictionary.
 */
DLLIMP lea_dict_entry*
lea_dict_iter_next(lea_dict_iter *iter);

/*
 * Returns the matching id of the log-file read in the session (useful if
 * you want to read this file again, after a logswitch is done).
 */
DLLIMP lea_logdesc*
lea_get_logfile_desc(OpsecSession *session);

/*
 * Retuns the number of the next log record
 */
DLLIMP int
lea_get_record_pos(OpsecSession *session);

/*
 * Suspends the LEA session
 */
DLLIMP int
lea_session_suspend(OpsecSession *session);

/*
 * Resumes a suspended LEA session
 */ 
DLLIMP int
lea_session_resume(OpsecSession *session);

/*************************************************************
 *
 *                      CLIENT LOGTRACK API
 *
 *************************************************************/
/*
 * Returns the filename of a file specified by its ID
 */
DLLIMP int
lea_get_filename_by_fileid(OpsecSession *session, char **pszFilename, int nFileId);

/*
 * Returns the ID of a file specified by its filename
 */
DLLIMP int
lea_get_fileid_by_filename(OpsecSession *session, int *pnNormalFileId, int *pnAccountFileId, char *szFilename);

/*
 * Returns information for the first file in the logtrack
 */
DLLIMP int
lea_get_first_file_info(OpsecSession *session, char **pszFilename, int *pnNormalFileId, int *pnAccountFileId);

/*
 * Returns information for the next file in the logtrack
 */
DLLIMP int
lea_get_next_file_info(OpsecSession *session, char **pszFilename, int *pnNormalFileId, int *pnAccountFileId);

/*
 * Returns information for the last file in the logtrack
 */
DLLIMP int
lea_get_last_file_info(OpsecSession *session, char **pszFilename, int *pnNormalFileId, int *pnAccountFileId);

/*
 * Returns the information (filename, ID) of the current file
 */
DLLIMP int
lea_get_current_filename_and_fileid(OpsecSession *session, char *szFilename, int *pnFileId);

/*************************************************************
 *
 *                      CLIENT COLLECTED LOGFILES API
 *
 *************************************************************/
typedef struct _lea_col_log_entry_t collected_file_info_t;

/*
 * Returns a handle to info of the first/last file in the logfile collection
 */
DLLIMP int
lea_get_first_collected_file_info( OpsecSession *session, collected_file_info_t **collected_file_info );

DLLIMP int
lea_get_last_collected_file_info( OpsecSession *session, collected_file_info_t **collected_file_info );

/*
 * Returns a handle to info of the next/prev file in the logfile collection
 */
DLLIMP int
lea_get_next_collected_file_info( OpsecSession *session, collected_file_info_t **collected_file_info );

DLLIMP int
lea_get_prev_collected_file_info( OpsecSession *session, collected_file_info_t **collected_file_info );


DLLIMP collected_file_info_t *
lea_collected_file_info_duplicate( collected_file_info_t *collected_file_info );

DLLIMP void
lea_collected_file_info_destroy( collected_file_info_t *collected_file_info );

/*
 * Returns a field value given a handle to info of the logfile collection
 */
DLLIMP lea_value_ex_t *
lea_get_collected_file_info_by_field_name( collected_file_info_t *collected_file_info, char *fld_name );

/* ----------------------------------------------------------------------
 *
 * Definitions for OPSEC usage.
 *
 * ----------------------------------------------------------------------*/

DLLIMP extern	OpsecEntityType	*LEA_CLIENT;
DLLIMP extern	OpsecEntityType	*LEA_SERVER;

/* ----------------------------------------------------------------------
 *  Definitions for the mode value passed to lea_new_session()
 * ----------------------------------------------------------------------*/
/* close the session when end of file is reached */
#define	LEA_OFFLINE	0

/* send the log file, then wait for additional log records */
#define	LEA_ONLINE	1

/* ----------------------------------------------------------------------
 *  Definitions for the pos value passed to lea_new_session()
 * ----------------------------------------------------------------------*/

#define LEA_AT_START 0
#define LEA_AT_END   1
#define LEA_AT_POS   2

/* ----------------------------------------------------------------------
 *  Definitions that can be passed to the log_filename value passed to
 * lea_new_session()
 * ----------------------------------------------------------------------*/
#define	LEA_NORMAL	"fw.log"
#define	LEA_ACCOUNT	"fw.log"
#define	LEA_AUDIT "fw.adtlog"

/* ----------------------------------------------------------------------
 *  Definitions that can be passed to the logtrack value passed to
 * lea_new_session()
 * ----------------------------------------------------------------------*/

#define LEA_FILENAME               0

#define LEA_NORMAL_FILEID          1
#define LEA_FIRST_NORMAL_FILEID    2
#define LEA_CURRENT_NORMAL_FILEID  3

#define LEA_ACCOUNT_FILEID         4
#define LEA_FIRST_ACCOUNT_FILEID   5
#define LEA_CURRENT_ACCOUNT_FILEID 6

#define LEA_NORMAL_FILENAME        7
#define LEA_ACCOUNT_FILENAME       8

#define LEA_SUSPENDED			   9
#define LEA_RESUMED				   10

/* Bitmasked LEA modes; 4 bits */
typedef enum {
	E_LEA_MODE_NORMAL,
	E_LEA_MODE_ACCOUNT,
	E_LEA_MODE_UNIFIED,
	E_LEA_MODE_SEMI,
	E_LEA_MODE_RAW,
	E_LEA_MODE_AUDIT,
	E_LEA_MODE_REPORTING
} eLeaOpenMode;

#define LEA_MODE_BIT_COUNT	4

/* Bitmasked LEA file open methods; 4 bits */
typedef enum {
	E_LEA_METHOD_FILENAME,
	E_LEA_METHOD_FILEID,
	E_LEA_METHOD_FIRST,
	E_LEA_METHOD_CURRENT
} eLeaOpenMethod;

#define LEA_METHOD_BIT_COUNT	4

/* Bitmasked LEA file open amounts; 1 bit for now */
typedef enum {
	E_LEA_METHOD_SINGLE,
	E_LEA_METHOD_ALL_LT
} eLeaOpenAmount;

#define LEA_NEW_MODE_BIT 0x80000000 /* using 1<<31 causes warnings on Solaris */

#define LEA_COMPOSE_MODE(amount, method, mode) (((LEA_NEW_MODE_BIT) | ((amount) << ((LEA_METHOD_BIT_COUNT) + (LEA_MODE_BIT_COUNT)))| \
													((method) << (LEA_MODE_BIT_COUNT)) | (mode)))



/* raw modes */
#define LEA_RAW_FILEID					LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILEID, E_LEA_MODE_RAW)
#define LEA_RAW_FILENAME				LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILENAME, E_LEA_MODE_RAW)
#define LEA_FIRST_RAW_FILEID			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FIRST, E_LEA_MODE_RAW)
#define LEA_CURRENT_RAW_FILEID			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_CURRENT, E_LEA_MODE_RAW)
#define LEA_RAW_SINGLE					LEA_COMPOSE_MODE(E_LEA_METHOD_SINGLE, E_LEA_METHOD_FILENAME, E_LEA_MODE_RAW)

/* unified modes */
#define LEA_UNIFIED_FILEID				LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILEID, E_LEA_MODE_UNIFIED)
#define LEA_UNIFIED_FILENAME			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILENAME, E_LEA_MODE_UNIFIED)
#define LEA_FIRST_UNIFIED_FILEID		LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FIRST, E_LEA_MODE_UNIFIED)
#define LEA_CURRENT_UNIFIED_FILEID		LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_CURRENT, E_LEA_MODE_UNIFIED)
#define LEA_UNIFIED_SINGLE				LEA_COMPOSE_MODE(E_LEA_METHOD_SINGLE, E_LEA_METHOD_FILENAME, E_LEA_MODE_UNIFIED)

/* semi-unified modes */
#define LEA_SEMI_FILEID					LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILEID, E_LEA_MODE_SEMI)
#define LEA_SEMI_FILENAME				LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILENAME, E_LEA_MODE_SEMI)
#define LEA_FIRST_SEMI_FILEID			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FIRST, E_LEA_MODE_SEMI)
#define LEA_CURRENT_SEMI_FILEID			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_CURRENT, E_LEA_MODE_SEMI)
#define LEA_SEMI_SINGLE					LEA_COMPOSE_MODE(E_LEA_METHOD_SINGLE, E_LEA_METHOD_FILENAME, E_LEA_MODE_SEMI)

/* audit modes */
#define LEA_AUDIT_FILEID				LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILEID, E_LEA_MODE_AUDIT)
#define LEA_AUDIT_FILENAME				LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FILENAME, E_LEA_MODE_AUDIT)
#define LEA_FIRST_AUDIT_FILEID			LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_FIRST, E_LEA_MODE_AUDIT)
#define LEA_CURRENT_AUDIT_FILEID		LEA_COMPOSE_MODE(E_LEA_METHOD_ALL_LT, E_LEA_METHOD_CURRENT, E_LEA_MODE_AUDIT)


/* Read modes for Reporting Tool */
#define LEA_SEMI_UNIFIED_FILEID			100
#define LEA_FIRST_SEMI_UNIFIED_FILEID	101
#define LEA_CURRENT_SEMI_UNIFIED_FILEID	102
#define LEA_SEMI_UNIFIED_FILENAME		103
#define LEA_SEMI_UNIFIED_SINGLE_FILE	104


/* ----------------------------------------------------------------------
 *  Definitions of return values returned from logtrack routines
 * ----------------------------------------------------------------------*/

typedef enum {
	LEA_SESSION_ERR = OPSEC_SESSION_ERR,
	LEA_SESSION_OK  = OPSEC_SESSION_OK,
	LEA_SESSION_NOT_AVAILABLE,
	LEA_SESSION_IT_END,
	LEA_SESSION_CURRENT_FILE,
	LEA_SESSION_FILE_PURGED
} LogtrackRetval;


DLLIMP OpsecSession *
lea_new_session(OpsecEntity *client, OpsecEntity *server, int mode,
				int logtrack, ...);

DLLIMP OpsecSession *
lea_new_suspended_session(OpsecEntity *client, OpsecEntity *server, int mode,
				int logtrack, ...);

/* ----------------------------------------------------------------------
 *  Server APIs
 * ----------------------------------------------------------------------*/
DLLIMP void *
lea_server_get_session_state(OpsecSession *session);

DLLIMP void
lea_server_set_session_state(OpsecSession *session, void *app_state);

/*
  client: after doing opsec_init_entity(...)
  
  server: after doing opsec_init_entitiy(...)
  
  mode: can be LEA_OFFLINE or LEA_ONLINE
  
  logtrack : see documentation (LEA_ACCOUNT_FILEID, LEA_NORMAL_FILEID etc.)

  long fileid : the fileid returned by lea_get_logfile_desc()
  
  char *log_filename: name of the file you want to read (only if logtrack == 0)
  
  int pos: can be LEA_AT_START or LEA_AT_END or LEA_AT_POS, and then you have
   to give the position, as well, returned by lea_get_record_pos()
  
  Return value: OpsecSession, or NULL for failure.
  */

#define	LEA_RECORD_HANDLER	_NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 2)
#define	LEA_DICT_HANDLER	_NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 3)
#define	LEA_EOF_HANDLER	    _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 4)
#define	LEA_SWITCH_HANDLER	_NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 5)
#define	LEA_SUSPEND_HANDLER	 _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 6)
#define LEA_RESUME_HANDLER   _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 7)
#define LEA_LOGTRACK_HANDLER _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 8)
#define LEA_FILTER_QUERY_ACK   _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 9)
#define LEA_COL_LOGS_HANDLER   _NEW_HANDLER_ATTR(OPSEC_LEA_CLIENT, 10)

#define OPSEC_COMM_SEND_LT       _NEW_OPSEC_SIGNAL(OPSEC_LEA_SERVER, 4)
#define OPSEC_COMM_SEND_COL_LOGS _NEW_OPSEC_SIGNAL(OPSEC_LEA_SERVER, 5)

#ifdef __cplusplus
}
#endif


#endif /* _h_lea_ */
