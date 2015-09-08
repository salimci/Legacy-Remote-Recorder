/* ///////////////////////////////////////////////////////////////////////////
CPMIClientDataTypes.h
Data types needed by CPMI clients only.

                     
			   THIS FILE IS PART OF Check Point OPSEC SDK


/////////////////////////////////////////////////////////////////////////// */
#ifndef CPMICLIENTDATATYPES_H_EBAF8F61912D11d3B7690090272CCB30
#define CPMICLIENTDATATYPES_H_EBAF8F61912D11d3B7690090272CCB30


#include "opsec/opsec.h"
#include "srvis/general/cperrors.h"
#include "cpmi/CPMIShared/CPMISharedDataTypes.h"
#include "CPMIExportClientAPI.h"



/* ////////////////////////////////////////////////////////////////////////// */
/**
 *
 * @name        CPMI Client side data types
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */



/* ////////////////////////////////////////////////////////////////////////// *
 *
 *                               handle definitions
 *
 * ////////////////////////////////////////////////////////////////////////// */
/** CPMI handle to database object */
DECLARE_CPMI_HANDLE (HCPMIDB);

/** CPMI handle to table object */
DECLARE_CPMI_HANDLE (HCPMITBL);

/** CPMI handle to class object */
DECLARE_CPMI_HANDLE (HCPMICLASS);

/** CPMI handle to field object */
DECLARE_CPMI_HANDLE (HCPMIFLD);

/** CPMI handle to result object */
DECLARE_CPMI_HANDLE (HCPMIRSLT);

/** CPMI handle to notification object */
DECLARE_CPMI_HANDLE (HCPMINOTIFYMSG);

/** CPMI handle to application object */
DECLARE_CPMI_HANDLE (HCPMIAPP);

/** CPMI handle to object iteration object */
DECLARE_CPMI_HANDLE (HCPMIITEROBJ);

/** CPMI handle to table iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERTBL);

/** CPMI handle to field field iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERFLD);

/** CPMI handle to element iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERELEM);

/** CPMI handle to class iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERCLASS);

/** CPMI handle to container iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERCNTR);

/** CPMI handle to ordered container iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERORDCNTR);

/** CPMI handle to database iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERDB);

/** CPMI handle to context object */
DECLARE_CPMI_HANDLE (HCPMICONTEXT);

/** CPMI handle to query object */
DECLARE_CPMI_HANDLE (HCPMIQUERY);

/** CPMI handle to context iteration object */
DECLARE_CPMI_HANDLE (HCPMIITERCONTEXT);

/** CPMI handle to application information object */
DECLARE_CPMI_HANDLE (HCPMIAPPINFO);

/* ////////////////////////////////////////////////////////////////////////// *
 *
 *                         enum definitions
 *
 * ////////////////////////////////////////////////////////////////////////// */


/** certificate possible states */
enum tagCPMI_CERT_STATE 
{
    /** uninitialized */
    eCPMI_CERT_STATE_UNINITIALIZED =  1,

    /** created but not pushed  */
    eCPMI_CERT_STATE_INITIALIZED,

    /** pushed succesfully */
    eCPMI_CERT_STATE_PUSHED
};
/** typedef for tagCPMI_CERT_STATE */
typedef enum tagCPMI_CERT_STATE tCPMI_CERT_STATE;



/** certificate possible states */
enum tagCPMI_LIC_STATE 
{
    /** uninitialized */
    eCPMI_LIC_STATE_UNINITIALIZED =  1,

    /** attached successfully  */
    eCPMI_LIC_STATE_ATTACHED,

    /** detached successfully  */
    eCPMI_LIC_STATE_DETATCHED
};
/** typedef for tagCPMI_LIC_STATE */
typedef enum tagCPMI_LIC_STATE tCPMI_LIC_STATE;




/* ////////////////////////////////////////////////////////////////////////// *
 *
 *                         callback definitions
 *
 * ////////////////////////////////////////////////////////////////////////// */
typedef eOpsecHandlerRC (*pfnCPMIBind_CB) (OpsecSession * pSession, 
                                           cpresult       stat, 
                                           void         * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMIObj_CB) (HCPMIDB    hDb  , 
                                          HCPMIOBJ   hObj , 
                                          cpresult   stat ,
                                          cpmiopid   opid , 
                                          void     * pvOpaque);

typedef eOpsecHandlerRC (*pfnCPMIGetCount_CB) (HCPMIDB    hDb  , 
                                               cpresult   stat ,
                                               unsigned int count,
                                          	   cpmiopid   opid , 
                                           	   void     * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMIDb_CB) (HCPMIDB    hDb   , 
                                         cpresult   stat  , 
                                         cpmiopid   opid  , 
                                         void     * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMIQuery_CB) (HCPMIDB     hDb    , 
                                            HCPMIRSLT   result , 
                                            cpresult    stat   , 
                                            cpmiopid    opid   , 
                                            void      * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMINotify_CB) (HCPMIDB          hDb    ,
                                             HCPMINOTIFYMSG   hMsg   ,
                                             cpresult         stat   ,
                                             cpmiopid         opid   ,
                                             void           * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMICertificate_CB) (HCPMIDB            hDb    ,
                                                  HCPMIAPP           hApp   ,
                                                  tCPMI_CERT_STATE   eState , 
                                                  cpresult           stat   ,
                                                  cpmiopid           opid   ,
                                                  void             * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMILicense_CB)     (HCPMIDB			 hDb    ,
                                                  HCPMIAPP			 hApp   ,
                                                  tCPMI_LIC_STATE    eState , 
                                                  cpresult			 stat   ,
                                                  cpmiopid			 opid   ,
                                                  void * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMIDbCertificate_CB) (HCPMIDB    hDb  , 
                                                      const char       * szSicName ,
                                                      char cert_bin_buf[64*1024] ,
                                                      int                nBufLen ,
                                                      cpresult           stat ,
                                                      cpmiopid           opid , 
                                                      void             * pvOpaque);


typedef eOpsecHandlerRC (*pfnCPMIDbUser_CB) (HCPMIDB         hDb ,
                                             const char    * szSicName ,
                                             tCPMI_CERT_STATE   eState ,
                                             int             iRefNum ,
                                             const char    * szAuthCode ,
                                             const char    * szAuthValidity ,
                                             const char    * szCertificate,
                                             int             nBufLen,
                                             cpresult        stat ,
                                             cpmiopid        opid , 
                                             void          * pvOpaque);



/**
The user function will be called separately for every application object from the application array.
@paramh Db: handle to db where installation occurred.
@param refApp: reference to the application for which there is a report.
@param state: the installation state of hApp.
@params stateResult = result for state.
@paramszMessage = extended string message
@isLast = is last notification for this state (CP_S_OK or CP_S_FALSE).
@paramopid = operation id as was returned from CPMIAppsInstallPolicy/CPMIAppsUninstallPolicy.
@parampvOpq = user data as was passed to CPMIAppsInstallPolicy/CPMIAppsUninstallPolicy  */
typedef eOpsecHandlerRC (*pfnCPMIPolicyInstall_CB) (HCPMIDB hDb,
                                                    HCPMIREF refApp,
                                                    tCPMI_POLICY_INSTALL_STATE state, 
                                                    cpresult stateResult, 
                                                    const char* szMessage,
                                                    cpresult isLast,
                                                    cpmiopid opid,
                                                    void * pvQpq);



typedef eOpsecHandlerRC (*pfnCPMIObjDispInfo_CB) (HCPMIDB   hDb , 
                                                  tCPMI_OBJECT_DISPLAY_INFO_RESULT  * pResults,
                                                  unsigned int nResultsCount,
                                                  cpresult stat ,
                                                  cpmiopid opid , 
                                                  void     * pvOpaque);



/* to be moved to public/shared H file in V_BUF */
struct tagCPMI_OBJ_OP 
{
	HCPMIREF hRefObj;  /* reference to the object */
	cpresult nRetCode; /* return code of the operation on the above object */
	const char * szRetMsg; /* extended err message from the operation on above object */
};
/** typedef for tagCPMI_OBJ_OPERATION */
typedef struct tagCPMI_OBJ_OP tCPMI_OBJ_OP;

typedef eOpsecHandlerRC (*pfnCPMIDb_CB_Ex) (HCPMIDB        hDb   ,  /*db handle*/
                                            tCPMI_OBJ_OP * arrRet, /* array of ret codes, one for each object */
                                            unsigned int   nArrLen, /* num of elements in arrRet */
                                            cpresult       statCode, /* overall success/failure code */
                                            cpmiopid       opid, 
                                            void         * pvOpaque);


/** simple callback */
typedef pfnCPMIBind_CB CPMIBind_CB;

/** object callback */
typedef pfnCPMIObj_CB CPMIObj_CB;

/** object count callback */
typedef pfnCPMIGetCount_CB CPMIGetCount_CB;

/** db callback */
typedef pfnCPMIDb_CB_Ex CPMIDb_Ex;

/** db callback */
typedef pfnCPMIDb_CB CPMIDb_CB;

/** query callback */
typedef pfnCPMIQuery_CB CPMIQuery_CB;

/** notification callback */
typedef pfnCPMINotify_CB CPMINotify_CB;

/** certificate operation callback */
typedef pfnCPMICertificate_CB CPMICertificate_CB;

/** license operation callback */
typedef pfnCPMILicense_CB CPMILicense_CB;

/** db certificate operation callback */
typedef pfnCPMIDbCertificate_CB CPMIDbCertificate_CB;

/** getreferrals operation callback */
typedef pfnCPMIQuery_CB CPMIGetReferrals_CB;

/** init and get_status user certificate callback */
typedef pfnCPMIDbUser_CB CPMIDbUser_CB;

/** install operation callback */
typedef pfnCPMIPolicyInstall_CB CPMIPolicyInstall_CB;

/** Object display information retrieval operation callback */
typedef pfnCPMIObjDispInfo_CB CPMIObjDispInfo_CB;

/*@}*/
#endif /*CPMICLIENTDATATYPES_H_EBAF8F61912D11d3B7690090272CCB30*/
/* ////////////////////// EOF CPMIClientDataTypes.h ////////////////////// */

