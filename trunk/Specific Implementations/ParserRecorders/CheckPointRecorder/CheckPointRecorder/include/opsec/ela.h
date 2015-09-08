#ifndef _ELA_H
#define _ELA_H

#include "opsec_export_api.h"
#include "opsec_vt_api.h"

#ifdef	__cplusplus
extern "C" {
#endif

/*
 * value types.
 * JOKER is a "wild-card". Used in formats and retrievers.
 */
typedef enum {ELA_VT_NONE=-1,
			  ELA_VT_JOKER=0,
			  ELA_VT_VOID,    
			  ELA_VT_INT,     /* 32 bit */
			  ELA_VT_IP,      /* 32 bit, network order */
			  ELA_VT_PORT,    /* 16 bit. unsigned */
			  ELA_VT_MASK,    /* no more than ELA_MAX_MASK_SIZE bits */
			  ELA_VT_INDEX,
			  ELA_VT_STRING,
			  ELA_VT_STRING64, /* no more than 64 char (include EOS) */
			  ELA_VT_FLOAT,
			  ELA_VT_BUFF,
			  ELA_VT_TIME,     /* GMT / UTC  */
			  ELA_VT_DURATION, /* In mili-seonds */
			  ELA_VT_LOGID,
			  ELA_VT_ELA_OBJ,  /* for internal use only */
			  ELA_VT_PROTO,    /* for protocol type */
			  ELA_VT_STRING_ID,
			  ELA_VT_LAST      /* we can add new type only directly before this */
		  } Ela_VtType;  

/*
 * Log types
 */
typedef enum {
	ELA_LOG_LOG   = (1<<0),
	ELA_LOG_CTL   = (1<<1),
	ELA_LOG_AUDIT = (1<<2),
	ELA_LOG_ACCOUNT = (1<<3)
 } Ela_LogType;


struct _ela_buf{int len; char buf[1];};
struct _ela_mask{int len;char mask[1];}; /* len in bits. lsb first */
#define ELA_MAX_MASK_SIZE 1024
#define ELA_SET_BIT(a,n)    (a[(n)/8] |=  (1<<((n)%8)))
#define ELA_CLR_BIT(a,n)    (a[(n)/8] &= ~(1<<((n)%8)))
#define ELA_CHECK_BIT(a,n) ((a[(n)/8] &   (1<<((n)%8)))?1:0)

/* we shall assume that when we know val we know its type as well */
typedef union _ela_val_{
	int n;      /* 32bit: INT,IP,INDEX,TIME,DURATION*/
	unsigned short h;  /* 16bit: PORT */
	char *str;
	struct _ela_mask *mask;
	struct _ela_buf  *buf_s;
	double dbl;  /* TBC : this is the only 8 byte type */
	void *ptr; /*OBJ,LOGID */
} Ela_val;


/*
 * PreDefinition for used structures and types to enable pointing to them
 */
typedef struct _ela_context_				Ela_CONTEXT;
typedef struct _ela_log_ 					Ela_LOG;
typedef struct _ela_ff_						Ela_FF;
typedef struct _ela_format_					Ela_FORMAT;
typedef struct _ela_resolve_information_ 	Ela_ResInfo;
typedef struct _ela_resolve_info_entry_ 	Ela_ResEntry;
typedef struct _ela_uid_  					Ela_UID;      /* has no use for now */
typedef struct _ela_log_id_					Ela_LogID;


/*
 * Type of resolving methods
 */
#define ELA_GENERIC_RES 0 /* res info is fully defined by edfault-res-info.*/
#define ELA_INDEX_RES  1 /* refering to the array with integer/index.
							Can be used to resolve a mask as array of inidices*/
#define ELA_ASSOC_RES  3 /* assosiative dictionary (the index is any VT)   */
#define ELA_FUNC_RES   4 /* Use a function to resolve from ref to val types -
							internal USE for now */
#define ELA_STRING_ID_RES   5 /* String values will be saved as string_id's -
							internal USE for now */


/*******************************************************
 *            Functions' ProtoTypes
 *******************************************************/


/* ELA log uid */
typedef struct  _Ela_LogUID  Ela_LogUID;

DLLIMP Ela_LogUID *ela_log_uid_create();
DLLIMP void        ela_log_uid_destroy( Ela_LogUID *ela_uid );
DLLIMP int         ela_log_set_uid( Ela_LOG *log, Ela_LogUID  *ela_uid );


/* ELA context */
DLLIMP Ela_CONTEXT *ela_context_create();
DLLIMP void         ela_context_destroy(Ela_CONTEXT *); /* which will destroy all under it...*/

/*
 * The APIs for adding an object to a context
 */
DLLIMP Ela_FF       *ela_ff_create(Ela_CONTEXT *, char *name, Ela_VtType type);
DLLIMP Ela_ResInfo  *ela_resinfo_create(Ela_CONTEXT *, char *, Ela_ResInfo *next, Ela_ResInfo *dflt, int type,...);
DLLIMP Ela_ResEntry *ela_resentry_add(Ela_CONTEXT *ctx, Ela_ResInfo *,...); /*each info type has its updates... */
/*
 * Log APIs
 */
DLLIMP Ela_LOG *ela_log_create(Ela_CONTEXT *); /* returns NULL on error */
DLLIMP void ela_log_destroy(Ela_LOG *); 
DLLIMP int  ela_log_add_raw_field(Ela_LOG *, char *name, Ela_VtType type, Ela_ResInfo *, ...);
DLLIMP int  ela_log_add_field(Ela_LOG *, Ela_FF *, Ela_ResInfo *, ...);
DLLIMP int  ela_log_remove_field(Ela_LOG *, Ela_FF *);
DLLIMP int  ela_log_set_type(Ela_LOG *, unsigned int);

/*
 * Log viewing
 */
DLLIMP int ela_log2str(Ela_LOG *log, char *s, int slen); /* temporary : */
DLLIMP int ela_log_ff_to_string(Ela_LOG *log, Ela_FF *ff, char *buf, int bufln);

#ifdef __cplusplus
}
#endif

#endif /* _ELA_H */
