/* /////////////////////////////////////////////////////////////////////////////
CPMIErrors.h
Definitions of cpmi error codes and strings.

                     
			   THIS FILE IS PART OF Check Point OPSEC SDK


///////////////////////////////////////////////////////////////////////////// */
#ifndef CPMIERR_H_83EEAE81482711D4B84D0090272CCB30
#define CPMIERR_H_83EEAE81482711D4B84D0090272CCB30


#include "srvis/general/cperrors.h"



/* define failure cpresult codes for cpmi (negative values) */
#ifndef CPMI_CPRESULT
#	define CPMI_CPRESULT(_code) \
	    ((cpresult) ( ( ((unsigned int)CP_SEVERITY_ERROR) << 31 ) | \
					( CP_FACILITY_CP << 16 )                      | \
					( (unsigned int)(0x1588 + (_code)) ) ) )
#endif /*CPMI_CPRESULT*/


/* define success cpresult codes for cpmi (positive values) */
#ifndef CPMI_CPRESULT_SUCCESS
#	define CPMI_CPRESULT_SUCCESS(_code) \
	    ((cpresult) ( ( ((unsigned int)CP_SEVERITY_SUCCESS) << 31 ) | \
					( CP_FACILITY_CP << 16 )                        | \
					( (unsigned int)(0x1588 + (_code)) ) ) )
#endif /*CPMI_CPRESULT_SUCCESS*/


/* declare cpmi errors codes */
#ifndef DECLARE_CPMI_STATUS
#	define DECLARE_CPMI_STATUS(_name,_code,_msg) enum { _name = CPMI_CPRESULT(_code) };
#endif /* DECLARE_CPMI_STATUS */


/* declare cpmi success codes */
#ifndef DECLARE_CPMI_STATUS_SUCCESS
#	define DECLARE_CPMI_STATUS_SUCCESS(_name,_code,_msg) enum { _name = CPMI_CPRESULT_SUCCESS(_code) };
#endif /* DECLARE_CPMI_STATUS_SUCCESS */


/*
 * Do NOT change error values !!!
 * Do NOT declare errors above 249 !!!
 */




/*
 * general errors (0-14)
 */
/*0x80041588, -2147215992*/
DECLARE_CPMI_STATUS (CPMI_E_VERSION_MISMATCH,      0,     "Versions Mismatch")

/*0x80041589, -2147215991*/
DECLARE_CPMI_STATUS (CPMI_E_UNKNOWN_CLIENT_REQUEST,1,     "Unknown Client Request")

/*0x8004158A, -2147215990*/
DECLARE_CPMI_STATUS (CPMI_E_SEND_AUDIT_LOG,		   2,     "Failed To Send Audit Log")

/*0x8004158B, -2147215989*/
DECLARE_CPMI_STATUS (CPMI_E_ADMIN_LOCKED,		   3,     "Administrator Is Locked")




/*
 * object related errors (25-49)
 */
/*0x800415A1, -2147215967*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_NOT_FOUND,         25,    "Object Not Found")

/*0x800415A2, -2147215966*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_CREATE,            26,    "Object Creation Failed")

/*0x800415A3, -2147215965*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_UPDATE,            27,    "Object Update Failed")

/*0x800415A4, -2147215964*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_DELETE,            28,    "Object Deletion Failed")

/*0x800415A5, -2147215963*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_REGISTERED,        29,    "Object Is Already Registered")

/*0x800415A6, -2147215962*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_VALIDATION,        30,    "Object Validation Failed")

/*0x800415A7, -2147215961*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_LOCK,              31,    "Object Lock Failed")

/*0x800415A8, -2147215960*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_LOCKED,            32,    "Object Already Locked")

/*0x800415A9, -2147215959*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_EXIST,             33,    "Object Already Exists")

/*0x800415AA, -2147215958*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_UPDATE_REFERENCES, 34,    "Object References Update Failed")

/*0x800415AB, -2147215957*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_DELETE_REFERENCES, 35,    "Object References Deletion Failed")

/*0x800415AC, -2147215956*/
DECLARE_CPMI_STATUS (CPMI_E_NOT_DELETEABLE,		   36,    "Object Can not be deleted")

/*0x800415AD, -2147215955*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_NOT_RENAMEABLE,		   37,    "Object can not be renamed")

/*0x800415AE, -2147215954*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_FILE, 38, "File doesn't exist or corrupted")

/*0x800415AF, -2147215953*/
DECLARE_CPMI_STATUS (CPMI_E_CANNOT_DELETE_LICENSE, 39,    "Failed to delete license objects")

/*0x800415B0, -2147215952*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_OBJ_NAME, 40,	  "Invalid object Name")

/*0x800415B1, -2147215951*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_INVALID_REFERENCE, 41,	  "Object contain invalid reference")

/*0x800415B2, -2147215950*/
DECLARE_CPMI_STATUS (CPMI_E_OBJ_NO_UID, 42,	  "No UID for object")

/*0x800415B3, -2147215949*/
DECLARE_CPMI_STATUS (CPMI_E_REFER_TO_DELETED_OBJ, 43,	  "Object contain reference to object that is about to be deleted by another client")




/*
 * db related errors (50-64)
 */
/*0x800415BA, -2147215942*/
DECLARE_CPMI_STATUS (CPMI_E_DB_LOCK,               50,    "Database Lock Error")

/*0x800415BB, -2147215941*/
DECLARE_CPMI_STATUS (CPMI_E_DB_BACKUP,             51,    "Database Backup Error")

/*0x800415BC, -2147215940*/
DECLARE_CPMI_STATUS (CPMI_E_DB_RESTORE,            52,    "Database Restore Error")

/*0x800415BD, -2147215939*/
DECLARE_CPMI_STATUS (CPMI_E_DB_NOT_FOUND,          53,    "Database Not Found")

/*0x800415BE, -2147215938*/
DECLARE_CPMI_STATUS (CPMI_E_DB_ALREADY_OPEN,       54,    "Database Already Open")

/*0x800415BF, -2147215937*/
DECLARE_CPMI_STATUS (CPMI_E_DB_NOT_OPEN,           55,    "Database Is Not Open")

/*0x800415C0, -2147215936*/
DECLARE_CPMI_STATUS (CPMI_E_DB_READ_ONLY,          56,    "Database Is Read Only")

/*0x800415C1, -2147215935*/
DECLARE_CPMI_STATUS (CPMI_E_DB_NOT_DIRTY,          57,    "Database Is Not Dirty")

/*0x800415C2, -2147215934*/
DECLARE_CPMI_STATUS (CPMI_E_DB_NOT_PERMITTED,      58,    "Only Provider-1 Super User Administrators can perform Restore")

/*0x800415C3, -2147215933*/
DECLARE_CPMI_STATUS (CPMI_E_DB_NO_SESSION_DESC,    59,    "A session description is required")



/*
 * query related errors (75-84)
 */
/*0x800415D3, -2147215917*/
DECLARE_CPMI_STATUS (CPMI_E_QUERY,                 75,    "General Query Error")

/*0x800415D4, -2147215916*/
DECLARE_CPMI_STATUS (CPMI_E_QUERY_NOT_FOUND,       76,    "Query Not Found")

/*0x800415D5, -2147215915*/
DECLARE_CPMI_STATUS (CPMI_E_QUERY_SYNTAX,          77,    "Bad Query Syntax")




/*
 * policy/status related errors (100-114)
 */
/*0x800415EC, -2147215892*/
DECLARE_CPMI_STATUS (CPMI_E_POLICY_NOT_FOUND,      100,   "Policy Not Found")

/*0x800415ED, -2147215891*/
DECLARE_CPMI_STATUS (CPMI_E_RULE_NOT_FOUND,        101,   "Rule Not Found")

/*0x800415EE, -2147215890*/
DECLARE_CPMI_STATUS (CPMI_E_AGENT_DISCONNECT,      102,   "Agent Disconnected")

/*0x800415EF, -2147215889*/
DECLARE_CPMI_STATUS (CPMI_E_BAD_REPLY,             103,   "Bad Status Report")

/*0x800415F0, -2147215888*/
DECLARE_CPMI_STATUS (CPMI_E_NOT_INSTALLED,         104,   "Product Not Installed")

/*0x800415F1, -2147215887*/
DECLARE_CPMI_STATUS (CPMI_E_UNTRUSTED,             105,   "Untrusted Host")

/*0x800415F2, -2147215886*/
DECLARE_CPMI_STATUS (CPMI_E_PROCESS_REQUEST,       106,   "Failed to proccess request")

/*0x415EC, 267756*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_INSTALL_WARNNING, 100, "Install Warning")

/*0x415ED, 267757*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_INSTALL_PARTIALLY_SUCCEEDED, 101, "Warning: Install operation partially succeeded")

/*0x415ED, 267758*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_INSTALL_INFO,     102, "Policy installation progress information")

/*
 * table related errors (125-134)
 */
/*0x80041605, -2147215867*/
DECLARE_CPMI_STATUS (CPMI_E_TBL_NOT_FOUND,         125,   "Table Not Found")

/*0x80041606, -2147215866*/
DECLARE_CPMI_STATUS (CPMI_E_TBL_LOCK,              126,   "Table Lock Failed")

/*0x80041607, -2147215865*/
DECLARE_CPMI_STATUS (CPMI_E_TBL_FILE_NOT_FOUND,		127,   "Table File Not Found")





/*
 * schema related errors (150-155)
 */
/*0x8004161E, -2147215842*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_CLASS,         150,   "Invalid Schema Class")

/*0x8004161F, -2147215841*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_FIELD,         151,   "Field Not Found")

/*0x80041620, -2147215840*/
DECLARE_CPMI_STATUS (CPMI_E_NO_VALUE,              152,   "Empty Field Value")

/*0x80041621, -2147215839*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_FIELD_VALUE,   153,   "Invalid Field Value")

/*0x80041622, -2147215838*/
DECLARE_CPMI_STATUS (CPMI_E_INVALID_FIELD_TYPE,   154,   "Invalid Field Type")



/*
 * CA error results (162-190)
 */
/*0x8004162A, -2147215830*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_INTERNAL_ERR,      162, "Internal error in Certificate Authority")

/*0x8004162B, -2147215829*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_GENERAL_ERR,       163, "General error in Certificate Authority")

/*0x8004162C, -2147215828*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_BAD_INPUT,         164, "Bad Input")

/*0x8004162D, -2147215827*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_CONN_ERR,          165, "Connection Error")

/*0x8004162E, -2147215826*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_PROTOCOL_ERR,      166, "Protocol Error")

/*0x8004162F, -2147215825*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_AUTH_FAILURE,      167, "Cannot establish connection with unauthenticated peer")

/*0x80041630, -2147215824*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_ENTITY_NOT_EXIST,  168, "The referred entity does not exist in the Certificate Authority")

/*0x80041631, -2147215823*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_ILLEGAL_DN,        169, "The DN specified is illegal")

/*0x80041632, -2147215822*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_WRITE_FILE_ERR,    170, "Cannot write data to file")

/*0x80041633, -2147215821*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_PUSH_CONN_ERR,     171, "Certificate cannot be pushed")

/*0x80041634, -2147215820*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_DB_CHANGED,        172, "Certificate Authority database changed")

/*0x80041635, -2147215819*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_MERGE_DB_ERR,      173, "Failed to merge Certificate Authority database")

/*0x80041636, -2147215818*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_COMMAND_ERR,       174, "Command Error")

/*0x80041639, -2147215815*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_EXIST,             177, "There is already a certificate with the specified details")

/*0x8004163A, -2147215814*/
DECLARE_CPMI_STATUS(CPMI_E_ICA_READONLY,          178, "Certificate Authority is in Read Only Mode")


/* 
 * management server related errors (175-176) 
 */
/*0x80041637, -2147215817*/
DECLARE_CPMI_STATUS (CPMI_E_MGMT_NOT_ACTIVE,      175,   "Management Server Not Active")




/*
 * opsec related errors (200-209)
 */
/*0x80041650, -2147215792*/
DECLARE_CPMI_STATUS (CPMI_E_OPSEC_SESSION,         200,   "General OPSEC Session Error")

/*0x80041651, -2147215791*/
DECLARE_CPMI_STATUS (CPMI_E_OPERATION_MEM_CLEANUP, 201,   "Operation Memory Cleanup is Needed")

/*0x80041652, -2147215790*/
DECLARE_CPMI_STATUS (CPMI_E_SESSION_NOT_CONNECTED, 202,   "Session Not Connected")

/*0x80041653, -2147215789*/
DECLARE_CPMI_STATUS (CPMI_E_SESSION_NOT_BOUNDED,   203,   "Session Not Bounded")

/*0x80041654, -2147215788*/
DECLARE_CPMI_STATUS (CPMI_E_SESSION_NOT_ESTABLISHED, 204, "Session Not Established")

/*0x80041655, -2147215787*/
DECLARE_CPMI_STATUS (CPMI_E_SIC, 205, "General SIC Error")

/*0x80041656, -2147215786*/
DECLARE_CPMI_STATUS (CPMI_E_SIC_CERT_NOT_VALID, 206, "The certificate is not valid yet")

/*0x80041657, -2147215785*/
DECLARE_CPMI_STATUS (CPMI_E_GENERAL_DB_CHANGED, 207, "General Database Change")



/*
 * license related errors (225-234)
 */
/*0x80041669, -2147215767*/
DECLARE_CPMI_STATUS (CPMI_E_LICENSE_NOT_FOUND,     225,   "License was not found")

/*0x8004166A, -2147215766*/
DECLARE_CPMI_STATUS (CPMI_E_LICENSE_EXIST,         226,   "License is already attached")

/*0x8004166B, -2147215765*/
DECLARE_CPMI_STATUS (CPMI_E_LICENSE_EXPIRED,       227,   "License is expired")

/*0x8004166C, -2147215764*/
DECLARE_CPMI_STATUS (CPMI_E_LICENSE_INVALID,       228,   "Invalid license. Please check the license validation code")

/*0x8004166D, -2147215763*/
DECLARE_CPMI_STATUS (CPMI_E_LICENSE_EXCEED,        229,   "Cannot attach license, it is already attached. Detach the license and try again")

/*0x41669, 267881*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_LICENSE_DIFMACHINE, 225, "Warning: The license IP doesn't match the host IP")



/*
 * CA successful results for users registration (245-254)
 */
/*0x30d40, 200000*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_REMOVED, 245, "User Has Been Removed")

/*0x30d41, 200001*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_REVOKED, 246, "User Has Been Revoked")

/*0x30d43, 200003*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_NOT_YET_VALID, 247, "User Certificate Is Initialized But Not Yet Valid")

/*0x30d44, 200004*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_AUTH_EXPIRED, 248, "Authentication Code of User Has Expired")

/*0x30d45, 200005*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_ALREADY_REVOKED, 249, "User Certificate Is Revoked")

/*0x30d46, 200006*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_ALREADY_EXPIRED, 250, "User Certificate Is Expired")

/*0x30d47, 200007*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_ENTITY_NOT_EXIST, 251, "User Certificate Was Either Not Initialized or Revoked/Expired")

/*0x30d48, 200008*/
DECLARE_CPMI_STATUS_SUCCESS(CPMI_S_ICA_USER_OTHER_CASE_EXISTS, 252, "Certificate With Same DN Is Already Exist For Another User")


#endif /* CPMIERR_H_83EEAE81482711D4B84D0090272CCB30 */
/* ///////////////////////////// EOF CPMIErrors.h ///////////////////////////// */



