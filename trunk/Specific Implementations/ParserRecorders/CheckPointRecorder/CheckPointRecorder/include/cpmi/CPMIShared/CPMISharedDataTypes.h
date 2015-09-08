/* /////////////////////////////////////////////////////////////////////////////
CPMISharedDataTypes.h
Definitions of CPMI basic data types.

                     
			   THIS FILE IS PART OF Check Point OPSEC SDK


///////////////////////////////////////////////////////////////////////////// */
#ifndef CPMISHAREDDATATYPES_H_4BABE6519B7511d3B7710090272CCB30
#define CPMISHAREDDATATYPES_H_4BABE6519B7511d3B7710090272CCB30


#include "opsec/opsec_vt_api.h"
#include "srvis/general/cpGUID.h"




/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    CPMI Basic Data Types 
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** definition of cpmi operation id */
typedef unsigned int cpmiopid;



/** cpmiopid error value */
enum { CPMIOPID_ERR  = -1 };



/** 
Basic CPMI handle type. Used to eliminate the need of casting in calls  
to CPMIHandleAddRef and CPMIHandleRelease.  */
typedef void* HCPMI; 



/* define CPMI different handles */
#define DECLARE_CPMI_HANDLE(name)  struct name##__ { int dummy; }; typedef struct name##__ *name



/** declare handle to object */
DECLARE_CPMI_HANDLE (HCPMIOBJ);


/** declare handle to reference */
DECLARE_CPMI_HANDLE (HCPMIREF);


/** declare handle to container */
DECLARE_CPMI_HANDLE (HCPMICNTR);


/** declare handle to ordered container */
DECLARE_CPMI_HANDLE (HCPMIORDERCNTR);

/** declare handle to file */
DECLARE_CPMI_HANDLE  (HCPMIFILE);



/** typedef for tagCPMI_UID */
typedef cpGUID tCPMI_UID;



/** CPMI Server Port Number */
enum tagCPMI_PORT_NUMBERS 
{ 
	/** Port number of CPMI server */
	eCPMI_SERVER_PORT = 18190,

	/** Port number of CPML server */
	eCPML_SERVER_PORT = 18270
};
/** typedef for tagCPMI_PORT_NUMBERS */
typedef enum tagCPMI_PORT_NUMBERS tCPMI_PORT_NUMBERS;



/** All possible database open mode */
enum tagCPMI_DB_OPEN_MODE 
{
    eCPMI_DB_OM_READ = 1, 
    eCPMI_DB_OM_WRITE
};
/** typedef for tagCPMI_DB_OPEN_MODE */
typedef enum tagCPMI_DB_OPEN_MODE tCPMI_DB_OPEN_MODE;



/** Notification Events */
enum tagCPMI_NOTIFY_EVENT 
{
	/** delete object notification */
	eCPMI_NOTIFY_DELETE            =  0x00000001,
	
	/** object update notification */
	eCPMI_NOTIFY_UPDATE            =  0x00000002,
	
	/** object rename notification */
	eCPMI_NOTIFY_RENAME            =  0x00000004,
	
	/** object create notification */
	eCPMI_NOTIFY_CREATE            =  0x00000008,
	
	/** policy install notification */
	eCPMI_NOTIFY_INSTALL_POLICY    =  0x00000010,
	
	/** policy un-install notification */
	eCPMI_NOTIFY_UNINSTALL_POLICY  =  0x00000020,
	
	/** AMON status change notification */
	eCPMI_NOTIFY_STATUS_CHANGE     =  0x00000040,

	/** HA db change change notification */
	eCPMI_NOTIFY_GENERAL_DB_CHANGE =  0x00000080,	

	/** HA changeover to Active succeeded */
	eCPMI_NOTIFY_CHANGE_TO_ACTIVE  =  0x00000100,

	/** HA changeover to Standby succeeded */
	eCPMI_NOTIFY_CHANGE_TO_STANDBY =  0x00000200,

	/** object unlock notification */
	eCPMI_NOTIFY_OBJ_UNLOCK		   =  0x00000400,

	/** table unlock notification */
	eCPMI_NOTIFY_TBL_UNLOCK		   =  0x00000800,
	
	/* db unlock notification */
	eCPMI_NOTIFY_DB_UNLOCK         =  0x00001000,

	/* CMA Synchronized by an SMC Backup */
	eCPMI_CMA_SYNCHRONIZED_BY_AN_SMC_BACKUP  =  0x00002000,

	/* system message notification */
	eCPMI_NOTIFY_SYSTEM_MESSAGE    =  0x00004000

};
/** typedef for tagCPMINotifyEvent */
typedef enum tagCPMI_NOTIFY_EVENT tCPMI_NOTIFY_EVENT;


/** System Message Types */
enum tagCPMI_SYSTEM_MESSAGE_TYPE 
{
	eCPMI_SYSMSGTYPE_NONE				   =  0x00000000,

	eCPMI_SYSMSGTYPE_GENERAL			   =  0x00000001,
		
	eCPMI_SYSMSGTYPE_KILL_CLIENT		   =  0x00000002
};
/** typedef for tagCPMI_MESSAGE_TYPE */
typedef enum tagCPMI_SYSTEM_MESSAGE_TYPE tCPMI_SYSTEM_MESSAGE_TYPE;


/** System Message Statuses */
enum tagCPMI_SYSTEM_MESSAGE_STATUS 
{
	eCPMI_SYSMSGSTAT_NONE = 0,

	eCPMI_SYSMSGSTAT_INFO,
		
	eCPMI_SYSMSGSTAT_WARNING,

	eCPMI_SYSMSGSTAT_ERROR
};
/** typedef for tagCPMI_SYSTEM_MESSAGE_STATUS */
typedef enum tagCPMI_SYSTEM_MESSAGE_STATUS tCPMI_SYSTEM_MESSAGE_STATUS;


/** Notification Flags */
enum tagCPMI_NOTIFY_FLAG 
{
	/** send the object on notifciation */
	eCPMI_FLAG_SEND_OBJECT      =  0x00000001,

	/** get notifications on changes done by myself */
	eCPMI_FLAG_GET_SELF_CHANGES =  0x00000002,

	/* RESERVED - for future use only */
	eCPMI_NOTIFY_FLAG_RESERVED1 = 0x00000004,
	eCPMI_NOTIFY_FLAG_RESERVED2 = 0x00000008,
	eCPMI_NOTIFY_FLAG_RESERVED3 = 0x00000010,
	eCPMI_NOTIFY_FLAG_RESERVED4 = 0x00000020,
	eCPMI_NOTIFY_FLAG_RESERVED5 = 0x00000040
};
/** typedef for tagCPMINotifyFlag */
typedef enum tagCPMI_NOTIFY_FLAG tCPMI_NOTIFY_FLAG;



/** define all types that can be used in a GetFieldValue or SetFieldValue operations */
enum tagCPMI_FIELD_VALUE_TYPE 
{
    /** undefined field value type. Used only by CPMI to indicate error when
     * returning tCPMI_FIELD_VALUE_TYPE from a function.  */
	eCPMI_FVT_UNDEFINED = 0,
	
	/** null terminated const string value */
	eCPMI_FVT_CTSTR,
	
	/** boolean value */
	eCPMI_FVT_BOOL,
	
	/** numeric value */
	eCPMI_FVT_NUM,

	/** unsigned numeric value */
	eCPMI_FVT_U_NUM,

    /** 64bit numeric value */
    eCPMI_FVT_NUM64,

	/** unsigned 64bit numeric value */
	eCPMI_FVT_U_NUM64,
	
	/** HCPMIOBJ value */
	eCPMI_FVT_OBJ, 
	
	/** HCPMIREF value */
	eCPMI_FVT_REF, 
	
	/** HCPMICNTR value */
	eCPMI_FVT_CNTR, 
	
	/** HCPMIORDERCNTR value */
	eCPMI_FVT_ORDERED_CNTR,

	/* RESERVED - for future use only */
	eCPMI_FVT_RESERVED1,
	eCPMI_FVT_RESERVED2,
	eCPMI_FVT_RESERVED3,
	eCPMI_FVT_RESERVED4,
	eCPMI_FVT_RESERVED5
};
/** typedef for tagCPMI_FIELD_VALUE_TYPE */
typedef enum tagCPMI_FIELD_VALUE_TYPE tCPMI_FIELD_VALUE_TYPE;



/** holds the value that is assigned to, or retrieved from a field */
struct tagCPMI_FIELD_VALUE 
{
	/** indicate which of the following members is valid */
	tCPMI_FIELD_VALUE_TYPE fvt;

    /* the actual data */
	union 
	{
		opsec_int64     u_n64Fv;      /* eCPMI_FVT_NUM64 value        */
		opsec_u_int64	u_u_n64Fv;    /* eCPMI_FVT_U_NUM64 value      */
		const char*     u_ctstrFv;    /* eCPMI_FVT_CTSTR value        */
		opsec_boolean   u_bFv;        /* eCPMI_FVT_BOOL value         */
		signed int      u_nFv;        /* eCPMI_FVT_NUM value          */
		unsigned int	u_u_nFv;      /* eCPMI_FVT_U_NUM value        */
		HCPMIOBJ        u_objFv;      /* eCPMI_FVT_OBJ value          */
		HCPMIREF        u_refFv;      /* eCPMI_FVT_REF value          */ 
		HCPMICNTR       u_cntrFv;     /* eCPMI_FVT_CNTR value         */
		HCPMIORDERCNTR  u_ordcntrFv;  /* eCPMI_FVT_ORDERED_CNTR value */
	} u_fv;
	
	/* next #defines are for easy union member access */
	#define n64Fv     u_fv.u_n64Fv
	#define un64Fv    u_fv.u_u_n64Fv
	#define ctstrFv   u_fv.u_ctstrFv
	#define bFv       u_fv.u_bFv
	#define nFv       u_fv.u_nFv
	#define unFv      u_fv.u_u_nFv
	#define objFv     u_fv.u_objFv
	#define refFv     u_fv.u_refFv
	#define cntrFv    u_fv.u_cntrFv
	#define ordcntrFv u_fv.u_ordcntrFv
};
/** typedef for tagCPMI_FIELD_VALUE */
typedef struct tagCPMI_FIELD_VALUE tCPMI_FIELD_VALUE;



/** all supported fields types */
enum tagCPMI_FIELD_TYPE 
{
	/** string field */
	eCPMI_FT_STR = 1,
	
	/** number field */
	eCPMI_FT_NUM, 
	
	/** unsigned number field */
	eCPMI_FT_U_NUM, 
	
	/** 64bit numeric */
	eCPMI_FT_NUM64,

	/** unsigned 64bit numeric */
	eCPMI_FT_U_NUM64,

	/** boolean field */
	eCPMI_FT_BOOL,
	
	/** owned field */
	eCPMI_FT_OWNED_OBJ,
	
	/** member field */
	eCPMI_FT_MEMBER_OBJ,
	
	/** linked field */
	eCPMI_FT_LINKED_OBJ,

	/* RESERVED - for future use only */
	eCPMI_FT_RESERVED1,
	eCPMI_FT_RESERVED2,
	eCPMI_FT_RESERVED3,
	eCPMI_FT_RESERVED4,
	eCPMI_FT_RESERVED5
};
/** typedef for tagCPMI_FIELD_TYPE */
typedef enum tagCPMI_FIELD_TYPE tCPMI_FIELD_TYPE;



/** All possible application types */
enum tagCPMI_APP_TYPE 
{
    /** firewall-1 application */
	eCPMI_AT_FW1               =  0x00000001,
    
    /** floodgate-1 application */
	eCPMI_AT_FG1               =  0x00000002,
    
    /** opsec application */
	eCPMI_AT_OPSEC             =  0x00000004,
    
    /** ha application */
	eCPMI_AT_HA                =  0x00000008,
	
    /** vpn-1 application */
	eCPMI_AT_VPN1              =  0x00000010,

	/** os application (cpshared) */
	eCPMI_AT_OS                =  0x00000020,

	/** management application  */
	eCPMI_AT_MANAGEMENT        =  0x00000040,

	/** wam application  */
	eCPMI_AT_WAM		       =  0x00000080,

	/** log server application  */
	eCPMI_AT_LOGSERVER         =  0x00000100,

	/** desktop policy server application*/
	eCPMI_AT_DTPS              =  0x00000200,

	/** wac application  */
	eCPMI_AT_WAC               =  0x00000400,

	/** ose device application  */
	eCPMI_AT_OSE               =  0x00000800,

	/** vpn dedicate application */
	eCPMI_AT_VPN_NET           =  0x00001000,

	/** sofaware gateway profile application */
	eCPMI_AT_SOFAWARE_GW_PROFILE      = 0x00002000,
	
    /* RESERVED for future use */
	
	eCPMI_AT_RESERVED2         =  0x00004000,
	eCPMI_AT_RESERVED3         =  0x00008000,
	eCPMI_AT_RESERVED4         =  0x00010000,
	eCPMI_AT_RESERVED5         =  0x00020000,
	eCPMI_AT_RESERVED6         =  0x00040000,
	eCPMI_AT_RESERVED7         =  0x00080000,
	eCPMI_AT_RESERVED8         =  0x00100000,
	eCPMI_AT_RESERVED9         =  0x00200000,
	eCPMI_AT_RESERVED10        =  0x00400000,
	eCPMI_AT_RESERVED11        =  0x00800000,
	eCPMI_AT_RESERVED12        =  0x01000000,
	eCPMI_AT_RESERVED13        =  0x02000000,
	eCPMI_AT_RESERVED14        =  0x04000000,
	eCPMI_AT_RESERVED15        =  0x08000000,
	eCPMI_AT_RESERVED16        =  0x10000000,
	eCPMI_AT_RESERVED17        =  0x20000000,
	eCPMI_AT_RESERVED18        =  0x40000000,
	eCPMI_AT_RESERVED19        =  0x80000000
};
/** typedef for tCPMI_APP_TYPE */
typedef enum tagCPMI_APP_TYPE tCPMI_APP_TYPE;


/** All posibble server high availability modes */
enum tagCPMI_SERVER_HA_MODE
{
	/** server is in standby mode */
	eCPMI_SERVER_HA_MODE_STANDBY = 0,

	/** server is in active mode */
	eCPMI_SERVER_HA_MODE_ACTIVE, 

	/** No Management HA */
	eCPMI_SERVER_HA_MODE_NONE, 
	eCPMI_SERVER_HA_MODE_RESERVED2, 
	eCPMI_SERVER_HA_MODE_RESERVED3, 
	eCPMI_SERVER_HA_MODE_RESERVED4, 
	eCPMI_SERVER_HA_MODE_RESERVED5
};
typedef enum tagCPMI_SERVER_HA_MODE tCPMI_SERVER_HA_MODE;


/** All possible server types */
enum tagCPMI_SERVER_RUN_TYPE
{
	/** Server is secondary server */
	eCPMI_SERVER_RUN_TYPE_SECONDARY = 0,

	/** Server is primary server */
	eCPMI_SERVER_RUN_TYPE_PRIMARY,
	eCPMI_SERVER_RUN_TYPE_RESERVED1,
	eCPMI_SERVER_RUN_TYPE_RESERVED2,
	eCPMI_SERVER_RUN_TYPE_RESERVED3,
	eCPMI_SERVER_RUN_TYPE_RESERVED4,
	eCPMI_SERVER_RUN_TYPE_RESERVED5
};
typedef enum tagCPMI_SERVER_RUN_TYPE tCPMI_SERVER_RUN_TYPE;


/** All possible installation types for server */
enum tagCPMI_SERVER_INSTALL_TYPE
{
	/** server installation type is management+module */
	eCPMI_SERVER_INSTALL_TYPE_MGMT_MODULE = 0,

	/** server installation type is only management */
	eCPMI_SERVER_INSTALL_TYPE_MGMT_ONLY = 1,
	eCPMI_SERVER_INSTALL_TYPE_RESERVED1, 
	eCPMI_SERVER_INSTALL_TYPE_RESERVED2, 
	eCPMI_SERVER_INSTALL_TYPE_RESERVED3, 
	eCPMI_SERVER_INSTALL_TYPE_RESERVED4, 
	eCPMI_SERVER_INSTALL_TYPE_RESERVED5 
};
typedef enum tagCPMI_SERVER_INSTALL_TYPE tCPMI_SERVER_INSTALL_TYPE;



/** All Possible management server types */
enum tagCPMI_SERVER_TYPE
{    
	/** Regular management server */
	eCPMI_SERVER_TYPE_CPM,

	/** Local mode CPMI server */
	eCPMI_SERVER_TYPE_CPML,

	/** CMA management */
	eCPMI_SERVER_TYPE_CMA,

	/** Provider-1 management */
	eCPMI_SERVER_TYPE_PV1,

	/** SiteManager-1 management */
	eCPMI_SERVER_TYPE_SM1,

	/** MLM management (Multi customer Log Module) */
	eCPMI_SERVER_TYPE_MLM,

	/** Log Management server */
	eCPMI_SERVER_TYPE_CLM, 
	
	eCPMI_SERVER_TYPE_RESERVED2, 
	eCPMI_SERVER_TYPE_RESERVED3, 
	eCPMI_SERVER_TYPE_RESERVED4, 
	eCPMI_SERVER_TYPE_RESERVED5,
	eCPMI_SERVER_TYPE_RESERVED6,
	eCPMI_SERVER_TYPE_RESERVED7,
	eCPMI_SERVER_TYPE_RESERVED8,
	eCPMI_SERVER_TYPE_RESERVED9
};
typedef enum tagCPMI_SERVER_TYPE tCPMI_SERVER_TYPE;



/** All states of policy installation */
enum tagCPMI_POLICY_INSTALL_STATE
{
	eCPMI_PIS_START = 0x1,
	eCPMI_PIS_VERIFY,	
	eCPMI_PIS_COMPILE,
	eCPMI_PIS_POLICY_STORE, 
	eCPMI_PIS_POLICY_TRANSFER,
	eCPMI_PIS_COMMIT,
	eCPMI_PIS_REMOVE, 
	eCPMI_PIS_DONE
};
typedef enum tagCPMI_POLICY_INSTALL_STATE tCPMI_POLICY_INSTALL_STATE;



/** All states of policy install options */
enum tagCPMI_POLICY_INSTALL_OPTION 
{
	eCPMI_PIO_ALL_OR_NONE    = 0x1,
	eCPMI_PIO_ALL_OR_NONE_CLUSTER = 0x2,
	eCPMI_PIO_VALIDATION_ONLY = 0x4,
	eCPMI_PIO_SD_INSTALL = 0x8,
	eCPMI_PIO_SD_UPDATE = 0x10
	
};
typedef enum tagCPMI_POLICY_INSTALL_OPTION tCPMI_POLICY_INSTALL_OPTION;



enum tagCPMI_CANDIDATE_OPERATION
{
	eCPMI_CO_INSTALL = 0x0,
	eCPMI_CO_INSTALL_BY_PERMISSION,
	eCPMI_CO_RESERVED_1,
	eCPMI_CO_RESERVED_2,
	eCPMI_CO_RESERVED_3,
	eCPMI_CO_RESERVED_4,
	eCPMI_CO_RESERVED_5,
	eCPMI_CO_RESERVED_6,
	eCPMI_CO_RESERVED_7,
	eCPMI_CO_RESERVED_8,
	eCPMI_CO_RESERVED_9,
	eCPMI_CO_RESERVED_10
};
typedef enum tagCPMI_CANDIDATE_OPERATION tCPMI_CANDIDATE_OPERATION;



enum tagCPMI_DISPLAY_INFO_OPTION
{
	eCPMIObjItself   = 0x1, /* Get the icon of the object itself */
	eCPMIRefferedObj = 0x2  /* Get the icon of objects that are referred by the object itself */
};
typedef enum tagCPMI_DISPLAY_INFO_OPTION tCPMI_DISPLAY_INFO_OPTION;



struct tagCPMI_OBJECT_DISPLAY_INFO_REQUEST
{
	const char	* szTableName;
	const char	* szQuery;	/* query string. Use NULL to get all objects. */
	tCPMI_DISPLAY_INFO_OPTION nOption;
};
typedef struct tagCPMI_OBJECT_DISPLAY_INFO_REQUEST tCPMI_OBJECT_DISPLAY_INFO_REQUEST;



struct tagCPMI_OBJECT_DISPLAY_INFO_RESULT
{
	HCPMIREF     hObjRef;       /* reference handle */
	char       * szObjColor;	/* textual representation of hexadecimal value */
	char       * szObjIconPath;	/* relative path to the object icon */
	char       * szObjToolTip;
};
typedef struct tagCPMI_OBJECT_DISPLAY_INFO_RESULT tCPMI_OBJECT_DISPLAY_INFO_RESULT;


/*@}*/
#endif /* CPMISHAREDDATATYPES_H_4BABE6519B7511d3B7710090272CCB30 */
/* /////////////////////// EOF CPMISharedDataTypes.h /////////////////////// */


