/* /////////////////////////////////////////////////////////////////////////
CPMIClientAPIs.h
Definitions of CPMI API's for CPMI clients.

                     
			   THIS FILE IS PART OF Check Point OPSEC SDK


///////////////////////////////////////////////////////////////////////// */
#ifndef CPMICLIENTAPIS_H_EB2C4486CC8611D2A3E30090272CCB30
#define CPMICLIENTAPIS_H_EB2C4486CC8611D2A3E30090272CCB30

#include <time.h>
#include "CPMIExportClientAPI.h"
#include "CPMIClientDataTypes.h"



/** 
@name Memory Management Rules
@memo Every handle that is returned as an out-parameter must be released with
      a call to CPMIHandleRelease when no longer in use (this includes handles
	  that are part of tCPMI_FIELD_VALUE).	  
      Every char* that is returned as an out-parameter must be released with
	  a call to CPMIFreeString. 
*/



/* ////////////////////////////////////////////////////////////////////////// */
/** 
 
@name                    Connection Management APIs 
*/
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMISessionNew Create an OpsecSession to be used by CPMI APIs. 
@param  pClientEntity [IN]  Entity representing the client. 
@param  pServerEntity [IN]  Entity representing the server. 
@param  nTimeout      [IN]  Timeout to wait for any request [miliseconds]. 
@param  ppSession     [OUT] Returned OpsecSession. 
@return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG, and CPMI_E_OPSEC_SESSION. */
CPMI_C_API 
cpresult CPMISessionNew (OpsecEntity *   pClientEntity, 
                         OpsecEntity *   pServerEntity, 
                         int             nTimeout, 
                         OpsecSession ** ppSession);




/** 
CPMISessionEnd ends a given session. 
@param  pSession [IN] Valid OpsecSession.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API
cpresult CPMISessionEnd (OpsecSession * pSession);
                                     



/** 
CPMISessionSetTimeout sets a new timeout for future session operations.  
@param  pSession [IN] Valid OpsecSession.
@param  nTimeout [IN] New timeout (in milliseconds).
@return CP_S_OK, CP_E_FAIL and CP_E_INVALIDARG  */
CPMI_C_API
cpresult CPMISessionSetTimeout (OpsecSession * pSession,
                                int            nTimeout);




/** 
CPMISessionGetTimeout gets the timeout in milliseconds. 
Used in given session operations. 
@param  pSession [IN] A valid OPSEC Session.
@param  pTimeout [IN] Returned timeout (in milliseconds). On failure,
					  *pTimeout is set to -1.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API
cpresult CPMISessionGetTimeout (OpsecSession * pSession,
                                int          * pTimeout);





/** 
CPMISessionBindUser binds the connection using user information. 
@param  pSession     [IN]  Valid OPSEC session.
@param  szUserName   [IN]  User name on database.
@param  szUserPasswd [IN]  User password on database.
@param  pfnCB        [IN]  User function to be called when operation is done.
@param  pvOpaque     [IN]  User data to pass to pfnCB.
@param  pOpId        [OUT] Address of cpmiopid variable to receive the operation id. 
                           *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMISessionBindUser (OpsecSession  * pSession     ,
                              const char    * szUserName   ,
                              const char    * szUserPasswd ,
                              CPMIBind_CB     pfnCB        ,
                              void          * pvOpaque     ,
                              cpmiopid      * pOpId);




/** 
CPMISessionBind binds the connection based on information given at the 
entity creation time. This allow binding using a certificate. 
@param  pSession [IN]  User password on database.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpid    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and CPMI_E_SESSION_NOT_CONNECTED. */
CPMI_C_API
cpresult CPMISessionBind (OpsecSession  * pSession, 
                          CPMIBind_CB     pfnCB   , 
                          void          * pvOpaque, 
                          cpmiopid      * pOpid);




/** 
CPMISessionIterDb gets an iteration handle to iterate over the opened databases 
on a given session.  
@param  pSession [IN]  OpsecSession used in opening the databases.
@param  phIter   [OUT] *phIter is set to NULL upon failure. 
                       *phIter should be released with CPMIHandleRelease when 
                       no longer needed.
@return CP_S_OK, CP_E_INVALIDARG. */
CPMI_C_API 
cpresult CPMISessionIterDb (OpsecSession * pSession,
                            HCPMIITERDB  * phIter);





/** 
CPMISessionSetCachePath sets a cache directory for the Client library and indicate to
the Client to use local caching. 
There is no need to call to CPMISessionUseCache prior CPMISessionSetCachePath.
By default, if this function is not called CPMI uses the './CPMICache/' directory.
@param  pSession [IN] A valid binded session.
@param  szPath   [IN] A path (directory name) for storing cache files.
@return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMISessionSetCachePath (OpsecSession * pSession,
                                  const char   * szPath);





/**
CPMISessionUseCache indicates whether or not to use local caching to improve network
performance. By default there is no use of the local cache so the client need 
to call this API in order to use the caching ability. 
@param pSession [IN] A valid binded session.
@param bUse     [IN] Flag with CP_S_OK or CP_S_FALSE.
@return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG. */
CPMI_C_API 
cpresult CPMISessionUseCache (OpsecSession * pSession,
                              cpresult       bUse);




/**
CPMISessionGetServerMajorVersion retrieves the server-side major version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSession  [IN]    A valid, bounded session.
@param pMajorVer [INOUT] Address of an unsigned int variable to receive 
                         the server major version.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API 
cpresult CPMISessionGetServerMajorVersion (OpsecSession * pSession, 
                                           unsigned int * pMajorVer);



/**
CPMISessionGetServerMinorVersion retrieves the server-side minor (feature pack) version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSession  [IN]    A valid, bounded session.
@param pMinorVer [INOUT] Address of an unsigned int variable to receive 
                         the server minor version.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API 
cpresult CPMISessionGetServerMinorVersion (OpsecSession * pSession, 
                                           unsigned int * pMinorVer);



/**
CPMISessionGetServerSPVersion retrieves the server-side service pack version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSession [IN]    A valid, bounded session.
@param pSPVer   [INOUT] Address of an unsigned int variable to receive the 
                        server service pack version.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API 
cpresult CPMISessionGetServerSPVersion (OpsecSession * pSession, 
										unsigned int * pSPVer);



/**
CPMISessionGetServerHFVersion retrieves the server-side hotfix version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSession [IN]    A valid, bounded session.
@param pHFVer   [INOUT] Address of an unsigned int variable to receive the 
                        server hotfix version.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API 
cpresult CPMISessionGetServerHFVersion (OpsecSession * pSession, 
										unsigned int * pHFVer);



/**
CPMISessionGetServerBuildNumber retreives the server-side build number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSession  [IN] A valid, bounded session.
@param pBuild    [INOUT] Address of an unsigned int variable to receive the 
					     server build number.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API 
cpresult CPMISessionGetServerBuildNumber (OpsecSession * pSession, 
                                          unsigned int * pBuild);



/**
CPMISessionGetServerHAMode retreives the High Availablitiy mode of the CPMI server. 
@param pSession [IN] A valid, bounded session.
@param pMode    [INOUT] The address of a tCPMI_SERVER_HA_MODE variable to receive the HA mode.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API   
cpresult CPMISessionGetServerHAMode (OpsecSession         * pSession,
                                     tCPMI_SERVER_HA_MODE * pMode);




/**
CPMISessionGetServerRunType retreives the server run type. 
@param pSession [IN] A valid, bounded session.
@param pType    [INOUT] The address of a tCPMI_SERVER_RUN_TYPE variable to receive the server type.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API    
cpresult CPMISessionGetServerRunType (OpsecSession          * pSession, 
                                      tCPMI_SERVER_RUN_TYPE * pType);




/**
CPMISessionGetServerInstallType retreives the server installation type. 
@param pSession [IN] A valid, bounded session.
@param pType    [INOUT] The address of a tCPMI_SERVER_INSTALL_TYPE variable to 
                        receive the server installation type.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API   
cpresult CPMISessionGetServerInstallType (OpsecSession              * pSession, 
                                          tCPMI_SERVER_INSTALL_TYPE * pType);




/**
CPMISessionGetServerType retrieves the type of the server, such as a 
SmartCenter Server, Customer Management Add-on Management, Provider-1/SiteManager-1 etc.
@param pSession [IN] A valid, bounded session.
@param pType    [INOUT] The address of a tCPMI_SERVER_TYPE variable to receive
                        the server type.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API cpresult 
CPMISessionGetServerType (OpsecSession      * pSession,
                          tCPMI_SERVER_TYPE * pType);
/**
CPMISessionIsSmcBackup retrieves if the server is an SMC Backup server. 
@param pSession   [IN] A valid, bounded session.
@return CP_S_OK, CP_S_FALSE or some internal error */
CPMI_C_API cpresult 
CPMISessionIsSmcBackup (OpsecSession      * pSession);


/*@}*/





/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Database Management APIs
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMIDbOpen opens the requested database.This database must be closed later using
CPMIDbClose before it can be re-opened.  
@param  pSession [IN]  A pointer to a valid session.
@param  szDbName [IN]  Database name. Reserved. Must be empty string ("").
@param  openMode [IN]  Open mode from tCPMI_DB_OPEN_MODE.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpid    [OUT] Address of the cpmiopid variable to be set to the 
                       operation ID. On failure, *pOpId is set to CPMIOPID_ERR.
@return CP_S_OK if successful. CP_E_INVALIDARG, or CPMI_E_OPSEC_SESSION. */
CPMI_C_API 
cpresult CPMIDbOpen (OpsecSession       * pSession, 
                     const char         * szDbName, 
                     tCPMI_DB_OPEN_MODE   openMode, 
                     CPMIDb_CB            pfnCB   , 
                     void               * pvOpaque, 
                     cpmiopid           * pOpid);
 




/**
CPMIDbClose closes the specified database and decrements the handle reference count. 
The database handle should not be released after the call to CPMIDbClose. 
Database that is still opened after the end-session-handler are automatically closed. 
@param  hDb [IN] a valid CPMI database handle.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIDbClose (HCPMIDB hDb);





/**
CPMIDbCreateObject creates a new object on the database. 
The object must be saved using CPMIObjUpdate in order to be stored in the database.  
@param hDb       [IN]  A valid CPMI database handle.
@param szObjType [IN]  Type of object to create (from sehcma)
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIDbCreateObject (HCPMIDB      hDb       ,
                             const char * szObjType , 
                             CPMIObj_CB   pfnCB     ,
                             void       * pvOpaque  ,
                             cpmiopid   * pOpId);





/**
CPMIDbGetOpenMode retrieves the specified database's working mode.  
@param hDb   [IN]  The database handle.
@param pMode [OUT] Address of the tCPMI_DB_OPEN_MODE variable to be set to the 
                  database's working mode, one of:
                  eCPMI_DB_OM_READ Read-only. 
                  eCPMI_DB_OM_WRITE Read-write. 
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE  */
CPMI_C_API 
cpresult CPMIDbGetOpenMode (HCPMIDB               hDb,
                            tCPMI_DB_OPEN_MODE * pMode);






/**
CPMIDbGetName gets the database name. 
@param  hDb     [IN]  A valid CPMI database handle.
@param  pszName [OUT] Address of string variable to receive the db name. 
                      *pszName is set to NULL on failure. 
                      *pszName should not be freed by caller.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIDbGetName (HCPMIDB       hDb,
                        const char ** pszName);




				
		
/**
CPMIDbGetTable gets a table from the database. Release the table with 
CPMIHandleRelease when not needed anymore.
@param  hDb       [IN]  A valid CPMI database handle.
@param  szTblName [IN]  Name of table to get.
@param  phTb      [OUT] Address of HCPMITBL receive the table handle. 
                        *phTbl is set to NULL upon failure. 
                        *phTbl should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CPMI_E_TBL_NOT_FOUND, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIDbGetTable (HCPMIDB      hDb       ,
                         const char * szTblName ,
                         HCPMITBL   * phTbl);





/**
CPMIDbIterTables gets an iteration handle to iterate over the database tables. 
Release the HCPMIITERTBL variable with CPMIHandleRelease when not needed anymore. 
@param  hDb    [IN]  A valid CPMI database handle
@param  phIter [OUT] Address of HCPMIITERTBL to receive the table iteration handle. 
                     *phIter is set to NULL upon failure. *phIter should be released 
					 when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE  */
CPMI_C_API
cpresult CPMIDbIterTables (HCPMIDB        hDb ,
                           HCPMIITERTBL * phIter);





/** 
CPMIDbRegisterEvent enables you to register a callback function for notifications 
on events that happen on the database. 
@param hDb       [IN]  A handle to the database
@param hTbl      [IN]  A handle to the table where notifications are requested. Pass NULL to
					   receive notifications from all tables.
@param hObj      [IN]  A handle to the object. If hObj is not NULL then only notifications that relate to hObj are
					   processed. If hObj is NULL then notifications from all objects in hTbl are
					   processed.
@param nEvents   [IN]  Bitmask of values from tCPMI_NOTIFY_EVENT specifying desired events.
@param nFlags    [IN]  Bitmask of values from tCPMI_NOTIFY_FLAG specifying additional flags.
@param  pfnCB    [IN]  User function to be called when the operation is done. To indicate a
					   successful registration pfnCB is called with a CP_S_OK status code and
					   hMgs is NULL. If registration fails then pfnCB is called with an error code
					   and hMgs is NULL. 
					   When session ends, hMsg will be NULL and the status will be CPMI_E_OPERATION_MEM_CLEANUP.
                       @param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIDbRegisterEvent (HCPMIDB         hDb      , 
                              HCPMITBL        hTbl     , 
                              HCPMIOBJ        hObj     , 
                              unsigned int    nEvents  , 
                              unsigned int    nFlags   , 
                              CPMINotify_CB   pfnCB    , 
                              void          * pvOpaque ,
                              cpmiopid      * pOpId);




/** 
CPMIDbUnregisterEvent cancel a previous call to CPMIDbRegisterevent. 
If successful, the callback function that was registered in the call to 
CPMIDbRegisterEvent will be called with an error code of CPMI_E_OPERATION_MEM_CLEANUP 
so resources can be released.
@param  hDb  [IN] A valid CPMI database handle.
@param  opId [IN] The cpmiopid that was returned in the call to
				  CPMIDbRegisterEvent.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIDbUnregisterEvent (HCPMIDB hDb, cpmiopid opId);




/** 
CPMIDbGetSession gets a pointer to the OPSEC Session used by any given database. 
@param  hDb       [IN]  The database handle.
@param  ppSession [OUT] Address of OpsecSession pointer to receive the 
                        OpsecSession pointer. 
                        *ppSession is set to NULL upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIDbGetSession (HCPMIDB         hDb, 
                           OpsecSession    ** ppSession);




/**
CPMIDbQueryApps queries the database for all applications of a specified type. 
@param  hDb      [IN]  The database handle.
@param  nAppType [IN]  Bitmask from tCPMI_APP_TYPE.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id. 
                       *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API
cpresult CPMIDbQueryApps (HCPMIDB        hDb      , 
                          unsigned int   nAppType ,
                          CPMIQuery_CB   pfnCB    , 
                          void         * pvOpaque ,
                          cpmiopid     * pOpId);




/** 
CPMIDbGetAppsStatus initiates a status request on a list of applications and 
returns the results.  
@param  hDb      [IN]  The database handle.
@param  rgApps   [IN]  An array of HCPMIAPPs.
@param  nCount   [IN]  The rgApps size.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id. 
                       *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIDbGetAppsStatus (HCPMIDB        hDb      , 
                              HCPMIAPP     * rgApps   , 
                              unsigned int   nCount   ,
                              CPMIQuery_CB   pfnCB    , 
                              void         * pvOpaque , 
                              cpmiopid     * pOpId);




/** 
CPMIDbIterClasses gets an iteration handle to iterate over the schema classes 
on a given database. 
@param  hDb    [IN]  The database handle.
@param  phIter [OUT] Address of HCPMIITERTBL to receive the iterator handle. 
                     *phIter is set to NULL upon failure. *phIter should be released when no longer
					 needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIDbIterClasses (HCPMIDB          hDb, 
                            HCPMIITERCLASS * phIter);





/** 
CPMIDbGetExtendedErrorMessage finds out if an error string for this operation exists, and
if so, returns it. In some cases, for example in CPMIObjValidate, a descriptive string value
is stored and associated with a cpmiopid. This function returns the string that is currently
associated with cpmiopid. These strings are valid only when the cpmiopid is valid which is
usually in the context of a callback.
@param  hDb  [IN]  hThe database handle.
@param  opId [IN]  A valid operation id, or 0 (zero) for last extended error message
                   that is not associated to a cpmiopid.
@param  psz  [OUT] Address of string variable to receive the returned string.
                   *psz is set to NULL upon failure. Free string using CPMIFreeString
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIDbGetExtendedErrorMessage (HCPMIDB     hDb, 
								        cpmiopid    opId, 
								        char     ** psz);





/** 
CPMIDbGetClassByName returns a handle to the requested schema class. 
@param  hDb         [IN]  The database handle.
@param  szClassName [IN]  The class name.
@param  phClass     [OUT] The address of HCPMICLASS variable which receives the 
                          returned handle. *phClass is set to NULL upon failure. 
                          *phClass should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CP_E_FAIL, CPMI_E_INVALID_CLASS. */
CPMI_C_API
cpresult CPMIDbGetClassByName (HCPMIDB      hDb ,
                               const char * szClassName,
                               HCPMICLASS * phClass);





/** 
CPMIDBQueryTables performs multiple queries on multiple tables from one API call, 
instead of multiple calls to CPMITblQueryObjects.
@param  hDb      [IN]  The database handle.
@param  hTbls    [IN]  An array of table handles.
@param  szQuery  [IN]  An array of query strings. Use NULL on each query string to get all objects.  
@param  queryNum [IN]  Number of table handles (and query strings) in the arrays
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_QUERY_SYNTAX,
CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIDbQueryTables (HCPMIDB        hDb       , 
							HCPMITBL     * hTbls     , 
	  					    const char   **pszQuerys , 
	  					    unsigned int   queryNum  , 
						    CPMIQuery_CB   pfnCB     , 
						    void         * pvOpaque  , 
	                        cpmiopid     * pOpId);





/** 
CPMIDBQueryTablesByContext performs multiple queries from multiple tables with context
reduction. This can replace separate calls to CPMITblQueryObjectsByContext.
@param  hDb      [IN]  The database handle.
@param  hTbls    [IN]  An array of table handles.
@param  szQuery  [IN]  An array of query strings. Use NULL to get all objects.  
@param  hContexts[IN]  An array of context handles. Use NULL for no context reduction.
@param  queryNum [IN]  Number of table handles (and query strings) in the arrays
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id. 
                       *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK if succeeded, 
        CP_E_INVALIDARG if the output argument pOpId is invalid or if the
                        arrays are 0 size, 
        CP_E_HANDLE if a NULL database handle or any NULL table handle in the
                    array was passed, 
        CPMI_E_QUERY_SYNTAX if any query has invalid syntax or some
                            unexpected internal error occurs */
CPMI_C_API 
cpresult CPMIDbQueryTablesByContext (HCPMIDB        hDb       , 
							         HCPMITBL     * hTbls     , 
	  					             const char   **pszQueries , 
                                     HCPMICONTEXT * hContexts ,
	  					             unsigned int   queryNum  , 
						             CPMIQuery_CB   pfnCB     , 
						             void         * pvOpaque  , 
	                                 cpmiopid     * pOpId);





/**
CPMIDbAcquireLockByRef attempts to aquire a lock on a given referenced object.
Guarantees exclusive access to an object by locking it. 
@param  hDb      [IN]  handle to the database in which the referenced object resides.
@param  hRef     [IN]  The reference handle.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR on fail.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIDbAcquireLockByRef (HCPMIDB     hDb,
                                 HCPMIREF    hRef,
                                 CPMIDb_CB   pfnCB,
                                 void	   * pvOpaque,
                                 cpmiopid  * pOpId);




/** 
CPMIDbReleaseLockByRef attempts to release locking on a given referenced object. 
@param  hDb  [IN] handle to the database in which the referenced object resides.
@param  hRef [IN] The referenced object handle.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIDbReleaseLockByRef (HCPMIDB hDb, HCPMIREF hRef);





/** 
CPMIDbDeleteObjByRef deletes the referenced object from the database. 
@param  hDb      [IN]  handle to the database where the referenced object resides.
@param  hRef     [IN]  The referenced handle.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIDbDeleteObjByRef (HCPMIDB     hDb,
                               HCPMIREF    hRef,
                               CPMIDb_CB   pfnCB, 
                               void      * pvOpaque,
                               cpmiopid  * pOpId);





/**
CPMIDbGetSchemaMajorVersion retrieves the schema major version. 
@param hDb       [IN]    A handle to the database object.
@param pMajorVer [INOUT] The address of an unsigned int variable to receive the schema
						 major version.
@return CP_S_OK, CP_E_INVALIDARG   */
CPMI_C_API
cpresult CPMIDbGetSchemaMajorVersion (HCPMIDB hDb, 
                                      unsigned int * pMajorVer);




/**
CPMIDbGetSchemaMinorVersion retrieves the schema minor version.  
@param hDb       [IN]    A handle to the database object.
@param pMinorVer [INOUT] The address of an unsigned int variable to receive the schema minor version
@return CP_S_OK, CP_E_INVALIDARG    */
CPMI_C_API
cpresult CPMIDbGetSchemaMinorVersion (HCPMIDB hDb, 
                                      unsigned int * pMinorVer);




/** 
CPMIDbCreateCertificate create a certificate for an administrator.
The attribute 'connection_state' of the administrator should be 'uninitialized' before running the command. 
Note, in case of success to attribute should be change BY THE CLIENT to 'communicating'.
@param hDb       [IN]  Handle to the database object.
@param hObj      [IN]  Handle to the object.
@param szPasswd  [IN]  The password for internal certificate authority (ICA).
@param pfnCB     [IN]  User function to be called when operation is done.
@param pvOpaque  [IN]  User data to pass to pfnCB.
@param pOpId     [OUT] Address of cpmiopid variable to receive the operation
                       id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIDbCreateCertificate (HCPMIDB      hDb ,
                                  HCPMIOBJ     hObj , 
                                  const char * szPasswd ,
                                  CPMIDbCertificate_CB pfnCB ,
                                  void      * pvOpaque ,
                                  cpmiopid  * pOpId);




/** 
CPMIDbRevokeCertificate revoke a certificate created for an administrator.
The attribute 'connection_state' of the administrator should be 'communicating' before running the command. 
Note, in case of success to attribute should be change BY THE CLIENT to 'uninitialized'. 
@param hDb      [IN]  handle to the database object
@param hObj     [IN]  handle to object
@param pfnCB    [IN]  User function to be called when operation is done.
@param pvOpaque [IN]  User data to pass to pfnCB.
@param pOpId    [OUT] Address of cpmiopid variable to receive the operation
                       id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIDbRevokeCertificate (HCPMIDB     hDb ,
                                  HCPMIOBJ    hObj ,
                                  CPMIDbCertificate_CB pfnCB , 
                                  void      * pvOpaque ,
                                  cpmiopid  * pOpId);

/** 
CPMIDbGetCandidates retrieves the candidates to opType operation.
The candidates return by the pfnCB.
The application objects handles (HCPMIAPP) can be retrieved from the resulting 
objects using CPMIObjGetAppHandle.

@param hDb       [IN]  Handle to the database object
@param opType    [IN]  Indicates to the operation type.
@param nAppType  [IN]  OR'ed of tagCPMI_APP_TYPE.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the operation
                       id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIDbGetCandidates (HCPMIDB     hDb ,
	                          tCPMI_CANDIDATE_OPERATION opType,
	                          unsigned int nAppType,
	                          CPMIQuery_CB pfnCB,
	                          void      * pvOpaque ,
	                          cpmiopid  * pOpId);


/** 
CPMIDbServerValidateObjects validate several objects at one api call. 
@param hDb 		   [IN] handle to the database object
@param arrObjects  [IN] array of handles to objects
@param nNumObjects [IN] num of elements in arrObjects
@param pfnCB       [IN] User function to be called when operation is done.
@param nFlags      [IN] Unused. Should be 0 (zero).
@param pvOpq 	   [IN] User data to pass to pfnCB.
@param pOpId       [OUT] Address of cpmiopid variable to receive the operation
                   id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG,CP_E_HANDLE, and case specific error codes   */
CPMI_C_API
cpresult CPMIDbServerValidateObjects(HCPMIDB hDb,
                                     HCPMIOBJ * arrObjects,
                                     unsigned int nNumObjects,
                                     unsigned int nFlags,
                                     CPMIDb_Ex pfnCB,
                                     void * pvOpq,
                                     cpmiopid * pOpId);


/**
 CPMIDbGetServerObj get the sever. 
 The object is returned in the user caller callback function.
 @param  hDb       [IN] handle to database
 @param  nFlags  [IN] additional flags. for future use, currently unused.
 @param pfnCB       [IN] User function to be called when operation is done.
 @param pvOpq 	   [IN] User data to pass to pfnCB.
 @param pOpId       [OUT] Address of cpmiopid variable to receive the operation id. 
 				*pOpId is set to CPMIOPID_ERR upon failure.
 @return CP_S_OK, CP_E_INVALIDARG,CP_E_HANDLE, and case specific error codes   */
CPMI_C_API cpresult 
CPMIDbGetServerObj (HCPMIDB      hDb,
                 unsigned int nFlags,
                 CPMIObj_CB   pfnCB,
                 void        * pvOpaque ,
                 cpmiopid    * pOpId);




/**
Similiar to CPMIObjGetReferrals. 
CPMIObjGetReferrals return a result of objects of class 'referral'.
Each 'referral' object in the returned result describe a reference from the 
input object to other object.
@param  HCPMIDB  [IN]  Valid database handle
@param  HCPMIREF [IN]  A reference to the object on which the operation is performed.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API    
cpresult CPMIDbGetObjReferrals (HCPMIDB     hDb,
                                HCPMIREF    hObjRef, 
                                CPMIGetReferrals_CB pfnCB, 
                                void         * pvOpaque, 
                                cpmiopid     * pOpId);



/*@}*/





/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Table Management APIs
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */



/**
CPMITblDeleteObj deletes an object from a given table. 
@param  hTbl      [IN]  The table handle.
@param  szObjName [IN]  The name of the object to be deleted.
@param  pfnCB     [IN]  User function to be called when operation is done.
@param  pvOpaque  [IN]  User data to pass to pfnCB.
@param  pOpId     [OUT] The address of the cpmiopid variable to receive the operation id. 
                        *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMITblDeleteObj (HCPMITBL     hTbl      , 
                           const char * szObjName , 
                           CPMIDb_CB    pfnCB     , 
                           void       * pvOpaque  ,
                           cpmiopid   * pOpId);



/**
CPMITblGetObjectCount get number of objects of a given table. 
@param  hTbl      [IN]  The table handle.
@param  pfnCB     [IN]  User function to be called when operation is done.
@param  nFlags    [IN]  Unused. Should be 0 (zero).
@param  pvOpaque  [IN]  User data to pass to pfnCB.
@param  pOpId     [OUT] The address of the cpmiopid variable to receive the operation id. 
                        *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG,CP_E_UNEXPECTED,CP_E_HANDLE */
CPMI_C_API 
cpresult CPMITblGetObjectCount (HCPMITBL     hTbl      , 
                                CPMIGetCount_CB pfnCB  , 
								unsigned int nFlags    ,
                           		void       * pvOpaque  ,
                           		cpmiopid   * pOpId);


/** 
CPMITblQueryObjects querys a given table for objects. 
@param  hTbl     [IN]  The table handle.
@param  szQuery  [IN]  The Query string. use NULL to get all objects.  
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to 0 upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_QUERY_SYNTAX if the 
        query has invalid syntax or some unexpected internal error occurs */
CPMI_C_API 
cpresult CPMITblQueryObjects (HCPMITBL       hTbl     , 
                              const char   * szQuery  , 
                              CPMIQuery_CB   pfnCB    , 
                              void         * pvOpaque , 
                              cpmiopid     * pOpId);




/** 
CPMITblQueryObjectsByContext querys a given table for objects with context reduction,
only fields approved by the context will remain in each object in the result.  
@param  hTbl     [IN]  HThe table handle.
@param  szQuery  [IN]  The Query string. use NULL to get all objects.  
@param  pfnCB    [IN]  The user function to be called when the operation is done.
@param  pvOpaque [IN]  The User data to pass to pfnCB.
@param  hContext [IN]  The Context handle.
@param  pOpId    [OUT] AThe address of the cpmiopid variable to receive the
					   operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_QUERY_SYNTAX if the 
        query has invalid syntax or some unexpected internal error occurs */
CPMI_C_API 
cpresult CPMITblQueryObjectsByContext (HCPMITBL       hTbl     , 
                                       const char   * szQuery  , 
                                       CPMIQuery_CB   pfnCB    , 
                                       void         * pvOpaque , 
                                       HCPMICONTEXT   hContext ,
                                       cpmiopid     * pOpId);

/**
CPMIQueryCreate create a query object for a given query syntax
@param szQuerySyntax [IN] Query string.
@param hTbl 		 [IN]  Handle to table.
@param phQuery		 [OUT] Address of HCPMIQUERY variable to receive the 
                      	   Query object.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API
cpresult CPMIQueryCreate (const char * szQuerySyntax,
						  HCPMITBL	   hTbl,
						  HCPMIQUERY * phQuery);


/**
CPMIObjQueryApply apply a query on object
@param hObj   [IN]  Handle to object.
@param hQuery [IN]  Handle to query object.
@return CP_S_OK, CP_S_FALSE or some internal error */
CPMI_C_API
cpresult CPMIObjQueryApply (HCPMIOBJ hObj,
							HCPMIQUERY hQuery); 

/** 
CPMITblGetName retrieves the name of the table. 
@param hTbl [IN]  Handle to table
@param psz  [OUT] The address of a string variable to be set to the table name. 
                  On failure, *psz is set to NULL. *psz should not be released.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMITblGetName (HCPMITBL      hTbl, 
                         const char ** psz);




/**
CPMITblIsWriteable checks for table working mode (read-only or read-write). 
@param hTbl [IN] The table handle.
@return CP_S_OK indicate read-write mode,
        CP_S_FALSE indicate read-only mode,
        CP_E_HANDLE on failure. */
CPMI_C_API 
cpresult CPMITblIsWriteable (HCPMITBL hTbl);




/**
CPMITblGetObj gets an object from a table. 
@param hTbl      [IN]  The table handle.
@param szObjName [IN]  The name of object to get.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_INVALIDARG, CPMI_E_DB_NOT_OPEN */
CPMI_C_API 
cpresult CPMITblGetObj (HCPMITBL     hTbl      , 
                        const char * szObjName , 
                        CPMIObj_CB   pfnCB     , 
                        void       * pvOpaque  , 
                        cpmiopid   * pOpId);




/**
CPMITblGetObjByContext gets an object from a table with context reduction, i.e. only fields
approved by the context will remain in the result object.  
@param hTbl      [IN]  The table handle.
@param szObjName [IN]  The name of object to get.
@param pfnCB     [IN]  The user function to be called when operation is done.
@param pvOpaque  [IN]  The user data to pass to pfnCB.
@param hContext  [IN]  The Context handle.
@param pOpId     [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK if succeeded, 
        CPMI_E_DB_NOT_OPEN if the database is not open,
        CP_E_INVALIDARG if the object name is empty or the output argument pOpId is invalid,
        CP_E_HANDLE if a NULL table handle was passed or some unexpected internal error occurs */
CPMI_C_API 
cpresult CPMITblGetObjByContext (HCPMITBL     hTbl      , 
                                 const char * szObjName , 
                                 CPMIObj_CB   pfnCB     , 
                                 void       * pvOpaque  , 
                                 HCPMICONTEXT hContext  ,
                                 cpmiopid   * pOpId);




/** 
CPMITblGetDb retrieves the handle of the parent database of the given table. 
@param  hTbl [IN]  The table handle.
@return pDb  [OUT] The address of the HCPMIDB variable to be set to the database
				   handle. On failure, *pDb is set to NULL. *pDb should be released
				   when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMITblGetDb (HCPMITBL  hTbl, 
                       HCPMIDB * pDb);



/** 
CPMITblGetDisplayString returns the table display string. 
@param  hTbl [IN]  The table handle.
@return pszDisplayStr  [OUT] The address of a string variable to be set to the table name. 
							 On failure, *psz is set to NULL. *psz should not be released.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMITblGetDisplayString (HCPMITBL  hTbl, 
                                  const char ** pszDisplayStr);


/*@}*/





/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Object Management APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMIObjDelete deletes the specified object.   
@param  hObj     [IN]  The object handle.
@param  pfnCB    [IN]  The function to be called once the object has been deleted.
@param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIObjDelete (HCPMIOBJ    hObj     , 
                        CPMIDb_CB   pfnCB    , 
                        void      * pvOpaque , 
                        cpmiopid  * pOpId);





/** 
CPMIObjUpdate updates the specified object in the database.   
@param  hObj     [IN]  The object handle.
@param  pfnCB    [IN]  The function to be called once the object has been updated.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMIObjUpdate (HCPMIOBJ    hObj     , 
                        CPMIDb_CB   pfnCB    , 
                        void      * pvOpaque ,
                        cpmiopid  * pOpId);




/** 
CPMIObjGetUid returns the object's unique ID. 
@param hObj [IN]  The object handle.
@param pUID [OUT] The address of the tCPMI_UID variable to be set to the object unique
				  ID. On failure *pUID will be assigned a NULL unique ID.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIObjGetUid (HCPMIOBJ    hObj, 
                        tCPMI_UID * pUID);



/**
CPMIObjGetUidAsString returns the object's unique ID as a string.
This API provide better performance than using CPMIObjGetUid and 
CPMIUidToString together.
@param hObj [IN]  The object handle.
@param hObj [OUT] The address of a string variable which receives the 
                  object unique ID formatted as a string. 
                  On failure, *psz is set to NULL. 
*/
CPMI_C_API 
cpresult CPMIObjGetUidAsString (HCPMIOBJ hObj, const char ** psz);




/** 
CPMIObjGetClass retrieves the schema class handle of the given object.   
@param  hObj    [IN]  The object handle.
@param  phClass [OUT] The address of the HCPMICLASS variable to be set to the 
                      object's class. On failure, *phClass is set to NULL. 
                      *phClass should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetClass (HCPMIOBJ     hObj, 
                          HCPMICLASS * phClass);




/** 
CPMIObjIsOfClass checks whether the object type matches the specified class.   
@param  hObj        [IN] The object handle.
@param  szClassName [IN] The class name.
@return CP_S_OK, CP_S_FALSE, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjIsOfClass (HCPMIOBJ     hObj, 
                           const char * szClassName);




/**
CPMIObjValidateName validate object name. If failed, *pszMsg will contain the error message.
@param pszMsg should then be release with CPMIFreeString */
CPMI_C_API
cpresult CPMIObjValidateName (HCPMIOBJ   hObj,
                              char ** pszMsg);



/** 
CPMIObjValidate is obsolete; use CPMIObjValidateEx instead.
@param  hObj     [IN]  The object handle.
@return CP_E_NOTIMPL. */
CPMI_C_API 
cpresult CPMIObjValidate (HCPMIOBJ hObj);



/** 
CPMIObjValidateEx validates that all of an object's fields have a valid value. When validation
fails, a descriptive string with the reason for failure is available by calling
CPMIDbGetExtendedErrorMessage.   
@param  hObj     [IN]  The object handle.
@param  pfnCB    [IN]  The function to be called once the object has been validated.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMIObjValidateEx (HCPMIOBJ    hObj     , 
	                       CPMIDb_CB   pfnCB    , 
	                       void      * pvOpaque ,
	                       cpmiopid  * pOpId);




/** 
CPMIObjValidateField validate an object's specific field value. 
@param  hObj [IN] The object handle.
@param  hFld [IN] The handle to the field to validate.
@return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIObjValidateField (HCPMIOBJ hObj,
                               HCPMIFLD hFld);




/** 
CPMIObjValidateFieldByName validate an object's specific field value. 
@param hObj      [IN] The object handle.
@param szFldName [IN] The name of the field to validate.
@return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, CP_E_HANDLE */
CPMI_C_API 
cpresult CPMIObjValidateFieldByName (HCPMIOBJ    hObj, 
                                     const char* szFldName);




/** 
CPMIObjGetFieldAsString gets the field value formatted as a string.  
@param  hObj      [IN]  The object handle.
@param  szFldName [IN]  The requested field name.
@param  psz       [OUT] The address of a string variable which receives the 
                        field value formatted as a string. 
                        *psz should be freed (using CPMIFreeString) when no longer needed.
                        On failure, *psz is set to NULL.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetFieldAsString (HCPMIOBJ      hObj     , 
                                  const char * szFldName ,
                                  char ** psz);





/**
CPMIObjGetField gets a handle to a field from an object.  
@param hObj      [IN]  The object handle.
@param szFldName [IN]  The requested field name.
@param phFld     [OUT] The address of HCPMIFLD variable which receives the 
                       field handle. *phFld is set to NULL upon failure. 
                       *phFld should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_INVALID_FIELD. */
CPMI_C_API 
cpresult CPMIObjGetField (HCPMIOBJ     hObj, 
                          const char * szFldName, 
                          HCPMIFLD   * phFld);





/** 
CPMIObjGetFieldValue gets a field value from an object. 
Upon successful function call, the field type can be determined from the pVal->fvt. 
pVal->objFv and pVal->refFv can be NULL even when the function is return successfully. 
Upon failure, prVal->fvt is set to eCPMI_FVT_UNDEFINED and all other members are invalid.   
@param hObj [IN]  TThe object handle.
@param hFld [IN]  The field handle.
@param pVal [OUT] The address of a tCPMI_FIELD_VALUE structure which receives the 
                  field value. pVal field's objFv, refFv, cntrFv and ordcntrFv 
                  should be release with CPMIHandleRelease when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_INVALID_FIELD. */
CPMI_C_API 
cpresult CPMIObjGetFieldValue (HCPMIOBJ            hObj, 
                               HCPMIFLD            hFld, 
                               tCPMI_FIELD_VALUE * pVal);





/** 
CPMIObjGetFieldValueByName gets a field value from an object. Upon successful function
call, the field type can be determined from the pVal->fvt. pVal->objFv and pVal->refFv
can be NULL even when the function is return successfully. Upon failure, prVal->fvt is set
to eCPMI_FVT_UNDEFINED and all other members are invalid.   
@param hObj      [IN]  The object handle.
@param szFldName [IN]  The field name.
@param pVal [OUT] The address of a tCPMI_FIELD_VALUE structure which receives the 
                  field value. pVal field's objFv, refFv, cntrFv and ordcntrFv 
                  should be release with CPMIHandleRelease when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_INVALID_FIELD. */
CPMI_C_API 
cpresult CPMIObjGetFieldValueByName (HCPMIOBJ            hObj, 
                                     const char        * szFldName,
                                     tCPMI_FIELD_VALUE * pVal);




/** 
CPMIObjSetFieldValueByName sets a value to field identifid by it's field name. 
If the field type on the schema does not match pNewVal->fvt the function fails.
@param hObj      [IN] The object handle.
@param szFldName [IN] The name of the field to change.
@param pNewVal   [IN] The address of a tCPMI_FIELD_VALUE variable containing 
                      the new field value.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, 
        CP_E_ACCESSDENIED if the database is not open in read/write mode, 
        CP_E_FAIL on failure or CPMI_E_INVALID_FIELD when the field is incompatible with the schema. */
CPMI_C_API 
cpresult CPMIObjSetFieldValueByName (HCPMIOBJ                  hObj, 
                                     const char              * szFldName, 
                                     const tCPMI_FIELD_VALUE * pNewVal);




/** 
CPMIObjSetFieldValue sets a value to a given field. 
If the field type on the schema does not match pNewVal->fvt the function fails. 
@param  hObj    [IN] The object handle.
@param  hFld    [IN] The field handle.
@param pNewVal  [IN] The address of a tCPMI_FIELD_VALUE structure containing the value
					  to set. The value must match the field type as in the schema.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, 
        CP_E_ACCESSDENIED if the database is not open in read/write mode, 
        CP_E_FAIL on failure or CPMI_E_INVALID_FIELD when the field is incompatible with the schema. */
CPMI_C_API
cpresult CPMIObjSetFieldValue (HCPMIOBJ                  hObj, 
                               HCPMIFLD                  hFld, 
                               const tCPMI_FIELD_VALUE * pNewVal);




/** 
CPMIObjResetFieldByName restore the field value to its default.  
@param hObj      [IN] The object handle.
@param szFldName [IN] The name of the field to reset.
@return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG, CPMI_E_HANDLE */
CPMI_C_API 
cpresult CPMIObjResetFieldByName (HCPMIOBJ    hObj, 
                                  const char* szFldName);





/** 
CPMIObjResetField restore the field value to its default.  
@param hObj      [IN] The object handle.
@param szFldName [IN] The handle of the field.
@return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG, CPMI_E_HANDLE */
CPMI_C_API
cpresult CPMIObjResetField (HCPMIOBJ hObj, 
                            HCPMIFLD hFld);





/** 
CPMIObjGetName retrieves the name of the designated object.  
@param  hObj [IN]  The object handle. 
@param  psz  [OUT] The pointer to be set to the object name. On failure, *psz is set
				   to NULL. *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetName (HCPMIOBJ      hObj, 
                         const char ** psz);





/** 
CPMIObjSetName sets the name of an object. Fail if the object is not renameable. 
This can be checked with CPMIObjIsRenameable. 
@param  hObj      [IN] The object handle.
@param  szObjName [IN] The object name to be set.
@return CP_S_OK, CP_E_HANDLE, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMIObjSetName (HCPMIOBJ     hObj, 
                         const char * szObjName);




/** 
CPMIObjGetDb retrieves a handle to the "parent" database of the object.  
@param  hObj [IN]  The object handle. 
@param  phDb [OUT] The address of the HCPMIDB variable to be set to the parent 
                   database's handle. On failure, *phDb is set to NULL. 
                   When no longer needed, *phDb should be released using CPMIHandleRelease.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetDb (HCPMIOBJ   hObj, 
                       HCPMIDB  * phDb);




/**
CPMIObjGetTbl retrieves a handle to the "parent" table of the object.  
@param hObj  [IN]  The object handle.
@param phTbl [OUT] The address of the HCPMITBL variable to be set to the parent 
                   table's handle. On failure, *phTbl is set to NULL.When no 
                   longer needed, this *phTbl should be released using CPMIHandleRelease
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetTbl (HCPMIOBJ   hObj, 
                        HCPMITBL * phTbl);





/** 
CPMIObjGetLastModifier retrieves the name of the last Administrator who modified the
object.  
@param  hObj [IN]  The object handle.
@param  psz  [OUT] The address of a string variable which receives the name of 
                   the last Administrator to modify the object. On failure, *psz 
                   is set to NULL. *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjGetLastModifier (HCPMIOBJ      hObj, 
                                 const char ** psz);




/** 
CPMIObjGetLastModifyHost retrieves the name of the machine on which the object was last
modified.  
@param  hObj [IN]  The object handle.
@param  psz  [OUT] The address of a string variable which receives the name of 
                   the machine on which the object was last modified. On failure, 
                   *psz is set to NULL. *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIObjGetLastModifierHost (HCPMIOBJ      hObj, 
                                     const char ** psz);




/** 
CPMIObjGetLastModificationTime retrieves the time the object was last modified.  
@param  hObj  [IN]  The object handle.
@param  pTime [OUT] The address of a time_t variable that receives the last 
                    modification time. *pTime is set to ((time_t)0) on failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIObjGetLastModificationTime (HCPMIOBJ   hObj, 
                                         time_t   * pTime);





/** 
CPMIObjAcquireLock guarantees exclusive access to an object by locking it. When an object
is locked, all its attributes, owned, linked, and member objects are locked as well.
If the given hObj is a new object that hasn't been updated, the error code
CPMI_E_OBJ_LOCK is returned immediately and pfnCB is not called. 
@param  hObj     [IN]  The object handle.
@param  pfnCB    [IN]  The function to be called once the object has been locked.
@param  pvOpaque [IN]  User-supplied data to pass to pfnCB.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CPMI_E_DB_NOT_OPEN. */
CPMI_C_API 
cpresult CPMIObjAcquireLock (HCPMIOBJ    hObj     , 
                             CPMIDb_CB   pfnCB    , 
                             void      * pvOpaque , 
                             cpmiopid  * pOpId);




/** 
CPMIObjReleaseLock unlocks an object locked by CPMIObjAcquireLock.  
@param  hObj [IN] The object handle.
@return CP_S_OK, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjReleaseLock (HCPMIOBJ hObj);




/** 
CPMIObjIsDeletable checks whether an object may be deleted.  
@param  hObj [IN] The object handle.
@return CP_S_OK, CP_S_FALSE, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjIsDeletable (HCPMIOBJ hObj);




/** 
CPMIObjGetAppHandle creates an application object from HCPMIOBJ.  
@param  hObj  [IN]  The application object handle.
@param  phApp [OUT] The address of an HCPMIAPP variable which receives the 
                    application object handle. On failure *phApp is set to NULL. 
                    *phApp should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMIObjGetAppHandle (HCPMIOBJ   hObj, 
                              HCPMIAPP * phApp);




/** 
CPMIObjIsOwned checks if the object is an owned object. 
@param  hObj  [IN]  The object handle.
@return CP_S_OK if the object is owned, CP_S_FALSE otherwise, CP_E_HANDLE on error. */
CPMI_C_API
cpresult CPMIObjIsOwned (HCPMIOBJ hObj);




/** 
CPMIObjClone clones an object, i.e. create a new local copy with a new HCPMIOBJ. 
@param  hInObj   [IN]  The object handle.
@param  phOutObj [OUT] The address of an HCPMIOBJ handle which receives the cloned
                       object. *phOutObj set to NULL on failure. *phOutObj should
                       be released when no longer needed.
@return CP_S_OK, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIObjClone (HCPMIOBJ   hInObj, 
                       HCPMIOBJ * phOutObj);




/** 
CPMIObjIsReduced checks to see if the object is currently reduced due to context reduction. 
@param  hObj  [IN]  The object handle.
@return CP_S_OK if succeeded, CP_E_HANDLE if a NULL reference handle was passed. */
CPMI_C_API
cpresult CPMIObjIsReduced (HCPMIOBJ hObj);




/** 
CPMIObjIsRenameable check if the object can be renamed. 
@param  hObj  [IN]  The object handle.
@return CP_S_OK if the object is renameable, CP_S_FALSE otherwise */
CPMI_C_API
cpresult CPMIObjIsRenameable (HCPMIOBJ hObj);



/**
CPMIObjGetReferrals return a result of objects of class 'referral'.
Each 'referral' object in the returned result describe a reference from the 
input object to other object.
@param HCPMIOBJ  [IN] The object handle for referral you wish to obtain.
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpId    [OUT] Address of cpmiopid variable to receive the 
                       operation id. *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API    
cpresult CPMIObjGetReferrals (HCPMIOBJ     hObj, 
                           CPMIGetReferrals_CB pfnCB    , 
                           void         * pvOpaque , 
                           cpmiopid     * pOpId);

/*@}*/





/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                   Field Managemenetment APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMIFldGetName retrieves the name of the given field.  
@param  hFld [IN]  The field handle.
@param  psz  [OUT] The address of a string variable which receives the field name. 
                   On failure, *psz is set to NULL. *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIFldGetName (HCPMIFLD      hFld, 
                         const char ** psz);




/** 
CPMIFldGetSize retrieves the specified field's size in bytes.  
@param  hFld  [IN]  The field handle.
@param  pSize [OUT] The address of a unsigned int variable which receives the 
                    field size, in bytes. On failure, *pSize is set to ((unsigned int)-1).
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIFldGetSize (HCPMIFLD   hFld, 
                         unsigned int     * pSize);





/** 
CPMIFldGetType retrieves the field's type. 
@param hFld  [IN]  The field handle.
@param pType [OUT] The address of the tCPMI_FIELD_TYPE variable to be set to 
                   the field type. Field type may be one of the following:
                   field                type
                   eCPMI_FT_UNDEFINED   Undefined (error).
                   eCPMI_FT_STR         Field is of string type.
                   eCPMI_FT_NUM         Field is of numeric type.
                   eCPMI_FT_U_NUM       Field is of unsigned int type.
                   eCPMI_FT_NUM64       Field is of 64 bit int type.
                   eCPMI_FT_U_NUM64     Field is of unsigned 64 bit int type.
                   eCPMI_FT_BOOL        Field is of boolean type.
                   eCPMI_FT_OWNED_OBJ   Field type is 'owned object'.
                   eCPMI_FT_MEMBER_OBJ  Field type is 'member object'.
                   eCPMI_FT_LINKED_OBJ  Field type is 'linked object'. 
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIFldGetType (HCPMIFLD           hFld, 
                         tCPMI_FIELD_TYPE * pType);




/** 
CPMIFldGetValidValues retrieves the valid values of a given field as it appears on the
schema.  
@param hFld [IN]  The field handle.
@param psz  [OUT] Returned valid values. On failure, *psz is set to NULL. 
                  *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIFldGetValidValues (HCPMIFLD      hFld, 
                                const char ** psz);




/** 
CPMIFldIsMultiple checks if the field type is container.  
@param hFld [IN] The field handle. 
@return CP_S_OK if the field type is container. 
        CP_S_FALSE if the field is not of type container and
        CP_E_HANDLE on failure. */
CPMI_C_API 
cpresult CPMIFldIsMultiple (HCPMIFLD hFld);





/** 
CPMIFldIsOrdered checks if the field type is ordered container.  
@param hFld [IN] The field handle.
@return CP_S_OK if the field type is ordered container. 
        CP_S_FALSE if the field is not of type ordered container
        CP_E_HANDLE on failure. */
CPMI_C_API 
cpresult CPMIFldIsOrdered (HCPMIFLD hFld);




/** 
CPMIFldGetDisplayString returns a string representing the field. If no string is present
NULL is returned. 
@param  hFld [IN]  The field handle.
@param  psz  [OUT] The address of a string variable which receives the field's 
                   string representation. On failure, *psz is set to NULL. 
                   *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIFldGetDisplayString (HCPMIFLD      hFld, 
								  const char ** pszStr);


/** 
 * get the default field value.
 * @param  hFld [IN]  the field handle
 * @param  psz  [OUT] returned default value. *psz set to NULL on failure
 * @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API
cpresult CPMIFldGetDefaultValue(HCPMIFLD      hFld, 
                                const char ** psz);

/*@}*/





/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Containers Management API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMICntrAdd adds an element to the end of the specified container.  
@param  hCnt [IN] The container handle.
@param  pFv  [IN] The address of a tCPMI_FIELD_VALUE variable which contains 
                  the element to be added.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMICntrAdd (HCPMICNTR                 hCnt , 
                      const tCPMI_FIELD_VALUE * pFv);





/** 
CPMICntrRemove removes the specified element from the container.  
@param  hCnt [IN] The container handle.
@param  pFv  [IN] The address of a tCPMI_FIELD_VALUE structure containing 
                  the element to be removed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMICntrRemove (HCPMICNTR                 hCnt , 
                         const tCPMI_FIELD_VALUE * pFv);



/** 
CPMICntrRemoveAll removes all elements from the container.  
@param  hCnt [IN] The container handle.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE, CP_E_FAIL. */
CPMI_C_API 
cpresult CPMICntrRemoveAll (HCPMICNTR                 hCnt);



/** 
CPMICntrGetCount retrieves the number of elements in a container.  
@param  hCnt   [IN]  The container handle.
@param  pCount [OUT] The address of the unsigned int variable to be set to the 
                     number of elements. On failure, *pCount is set to ((unsigned int)-1).
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.   */
CPMI_C_API 
cpresult CPMICntrGetCount (HCPMICNTR      hCnt , 
                           unsigned int * pCount);





/** 
CPMICntrIterElements retrieves a handle to an element iterator object. 
@param  hCnt   [IN]  The container handle.
@param  phIter [OUT] The address of the HCPMIITERCNTR variable to be set to 
                     the element iterator handle. On failure, *phIter is set to NULL. 
                     *phIter should be released when done.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMICntrIterElements (HCPMICNTR       hCnt , 
                               HCPMIITERCNTR * phIter);





/** 
CPMICntrGetElementType retrieves the type of elements stored in the container.   
@param hCnt  [IN]  The container handle.
@param pType [OUT] The address of tCPMI_FIELD_TYPE.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMICntrGetElementType (HCPMICNTR          hCnt , 
								 tCPMI_FIELD_TYPE * pType);

/*@}*/



/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                  Ordered Containers Management API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
CPMIOrderCntrAdd adds an element to the end of an ordered container.  
@param  hCnt  [IN] hThe ordered container handle.
@param  pElem [IN] The address of a tCPMI_FIELD_VALUE structure containing the 
                   element to be added.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIOrderCntrAdd (HCPMIORDERCNTR            hCnt, 
                           const tCPMI_FIELD_VALUE * pElem); 





/** 
CPMIOrderCntrAddAt adds an element at a specific location in the ordered container. 
@param  hCnt   [IN] The ordered container handle.
@param  nIndex [IN] The index of the location where the element is to be added (zero
					based).
@param  pElem  [IN] The address of a tCPMI_FIELD_VALUE structure containing 
                    the element to be added.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIOrderCntrAddAt (HCPMIORDERCNTR            hCnt  ,
                             unsigned int              nIndex,
                             const tCPMI_FIELD_VALUE * pElem);





/** 
CPMIOrderCntrRemove removes an element from the container.  
@param  hCnt  [IN] The ordered container handle.
@param  pElem [IN] The address of a tCPMI_FIELD_VALUE structure containing 
                   the element to be removed. 
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.*/
CPMI_C_API 
cpresult CPMIOrderCntrRemove (HCPMIORDERCNTR            hCnt,
                              const tCPMI_FIELD_VALUE * pElem);





/** 
CPMIOrderCntrRemoveAt removes the element from a specified location in the container.  
@param  hCnt   [IN] The ordered container handle.
@param  nIndex [IN] The index to the element location (zero based).
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.   */
CPMI_C_API 
cpresult CPMIOrderCntrRemoveAt (HCPMIORDERCNTR hCnt,
                                unsigned int   nIndex);





/** 
CPMIOrderCntrGetAt retrieves the element at the specified location in the container.   
@param  hCnt   [IN]  handle to ordered container
@param  nIndex [IN]  he index to the element location (zero based).
@param  pVal   [OUT] Address of tCPMI_FIELD_VALUE variable to receive the element value
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIOrderCntrGetAt (HCPMIORDERCNTR      hCnt  ,
                             unsigned int        nIndex,
                             tCPMI_FIELD_VALUE * pVal);





/**
CPMIOrderCntrIndexOf retrieves the index of the location of the element in the ordered
container.   
@param hCnt   [IN]  The ordered container handle.
@param pElem  [IN]  The address of a tCPMI_FIELD_VALUE structure containing the element.
@param pIndex [OUT] The address of an unsigned int variable to be set to the 
                    position of the element. On failure, *pIndex is set to ((unsigned int)-1).
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIOrderCntrIndexOf (HCPMIORDERCNTR            hCnt ,
                               const tCPMI_FIELD_VALUE * pElem,
                               unsigned int            * pIndex);





/** 
CPMIOrderCntrGetCount retrieves the number of elements in the ordered container. 
@param  hCnt   [IN]  The ordered container handle.
@param  pCount [OUT] The address of a unsigned int variable to be set to the number 
                     of elements in the container. 
                     On failure, *pCount is set to ((unsigned int)-1).
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE.  */
CPMI_C_API 
cpresult CPMIOrderCntrGetCount (HCPMIORDERCNTR   hCnt, 
                                unsigned int   * pCount);





/** 
CPMIOrderCntrIterElements retrieves a handle to an ordered container iterator. 
@param  hCnt   [IN]  The ordered container handle.
@param  phIter [OUT] The address of the HCPMIITERCNTR variable to be set to the 
                     iterator handle. On failure, *phIter is set to NULL. 
                     *phIter should be released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIOrderCntrIterElements (HCPMIORDERCNTR      hCnt,
                                    HCPMIITERORDCNTR  * phIter);





/** 
CPMIOrderCntrGetElementType gets the type of the elements stored in the ordered
container.
@param hOrdCnt [IN] The ordered container handle.
@param pType [OUT] The address of a tCPMI_FIELD_TYPE enum to receive the elements type.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIOrderCntrGetElementType (HCPMIORDERCNTR hOrdCnt , 
									 tCPMI_FIELD_TYPE * pType);
/*@}*/






/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Schema Class Management APIs
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
 CPMIClassGetTableName get the class table name. 
 @param  hClass [IN]  Handle to class.
 @param  psz    [OUT] Returned class name. *psz is set to NULL on failure. *psz should not
					  be freed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIClassGetTableName (HCPMICLASS    hClass,
                                const char ** psz);

						   
/** 
  CPMIClassGetName get the class name. 
  @param  hClass [IN]  The class handle.
  @param  psz    [OUT] Returned class name. *psz is set to NULL on failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIClassGetName (HCPMICLASS    hClass,
                           const char ** psz);


/** 
  CPMIClassIsDerivedFrom checks if the specified schema class is derived from another
  schema class. 
  @param  hClass      [IN] The class handle.
  @param  szClassName [IN] The name of the base class which the given class checks against.
  @return CP_S_OK,CP_S_FALSE, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIClassIsDerivedFrom (HCPMICLASS   hClass, 
                                 const char * szClassName);


/** 
CPMIClassIterFields retrieves a handle to a field iterator object. 
@param  hClass [IN]  The class handle.
@param  phIter [OUT] The address of the HCPMIITERFLD variable to be set to the iterator
                     handle. On failure, *phIter is set to NULL. *phIter should be
					 released when no longer needed.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIClassIterFields (HCPMICLASS     hClass,
                              HCPMIITERFLD * phIter);



/** 
CPMIClassGetDisplayString gets the class display string. If the given 
class does not have a display string, the returned string will be NULL.
@param  hClass [IN]  The class handle.
@param  psz    [OUT] The returned class display string. If there is none, *psz is set to
                     NULL. *psz should not be freed.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIClassGetDisplayString (HCPMICLASS   hClass, 
								    const char ** psz);



/** 
CPMIClassGetFieldByName return a field handle by field name. 
If the field does not exist the returned handle will be NULL.
@param	hClass      [IN]  The class handle.
@param  szFieldName [IN]  The requested field name.
@param	phField     [OUT] The returned field. If there is none, *phField is set to
					      NULL. *psz should be released with CPMIHandleRelease.
@return CP_S_OK, CPMI_E_INVALID_FIELD, CP_E_INVALIDARG */
CPMI_C_API 
cpresult CPMIClassGetFieldByName(HCPMICLASS	 hClass, 
                                 const char * szFieldName,
                                 HCPMIFLD * phField);



/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                      Database iteration APIs
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
  CPMIIterDbGetNext retrieves a handle to the next database in the session, if this is the
  first call to the function, the first database handle is retrieved.
  @param  hIter [IN]  The database iterator handle.
  @param  phDb [OUT] The address of the HCPMIDB variable to be set to the database
					 handle. When done, On failure *phDb is set to NULL. *phDb
					 should be released when no longer needed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterDbGetNext (HCPMIITERDB   hIter,
                            HCPMIDB     * phDb);


/** 
  CPMIIterDbGetCount retrieves the number of databases on the current iteration. 
  @param  hIter  [IN]  The database iterator handle.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of databases. *pCount is set to -1 on failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterDbGetCount (HCPMIITERDB   hIter,
                             unsigned int * pCount);


/** 
 CPMIIterDbIsEmpty checks if the database iteration object is empty. 
 @param  hIter [IN] The database iteration handle.
 @return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterDbIsEmpty (HCPMIITERDB hIter);


/** 
 CPMIIterDbIsDone checks if the iteration has been completed. 
 @param  hIter [IN] The database iterator handle.
 @return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterDbIsDone (HCPMIITERDB hIter);


/** 
 CPMIIterDbRestart starts the iteration over from the beginning. This function can be
 called anytime during the iteration.
 @param  hIter [IN] The database iterator handle.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterDbRestart (HCPMIITERDB hIter);


/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Table iteration APIs
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMIIterTblGetNext retrieves a handle to the next table in the database, if this is the first
 call to the function, the first table handle is retrieved. When no longer needed, this
 handle should be freed using CPMIHandleRelease.
 @param  hIter [IN]  HThe handle to a table iteration object.
 @param  phTbl [OUT] Address of HCPMITBL variable that receives the 
                     table handle. *phTbl is set to NULL on failure.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterTblGetNext (HCPMIITERTBL   hIter,
                             HCPMITBL     * phTbl);


/** 
 CPMIIterTblGetCount retrieves the number of tables in the given database.
 @param  hIter  [IN]  The handle to a table iteration object.
 @param  pCount [OUT] The address of the unsigned int variable to be set to the number of tables. On failure,
 *pCount is set to ((unsigned int)-1).
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterTblGetCount (HCPMIITERTBL   hIter,
                              unsigned int * pCount);


/** 
  CPMIIterTblIsEmpty checks whether the table is empty of objects. 
  @param  hIter [IN] The handle to a table iteration object.
  @return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterTblIsEmpty (HCPMIITERTBL hIter);


/** 
  CPMIIterTblIsDone tests whether the iteration has been completed. 
  @param  hIter [IN] The handle to a table iteration object.
  @return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterTblIsDone (HCPMIITERTBL hIter);


/** 
  CPMIIterTblRestart starts the iteration over from beginning. This function can be called
  anytime during the iteration.
  @param  hIter [IN] The handle to a table iteration object.x
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterTblRestart (HCPMIITERTBL hIter);


/*@}*/



/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Object Iteration APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
  CPMIIterObjGetNext retrieves a handle to the next object in the iteration, if this is the
  first call to the function, the first object handle is retrieved.
  @param  hIter [IN]  The handle to the object iterator, as returned by CPMIResultIterObj.
  @param  phObj [OUT] The address of the HCPMIOBJ variable which receives the object
					  handle. On failure, *phObj is set to NULL. *phObj should be released.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API
cpresult CPMIIterObjGetNext (HCPMIITEROBJ   hIter,
                             HCPMIOBJ     * phObj);


/** 
  CPMIIterObjGetCount retrieves the number of objects in the query result. 
  @param  hIter  [IN]  The handle to the object iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of objects. On failure, *pCount is set to ((unsigned int)-1).
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterObjGetCount (HCPMIITEROBJ   hIter,
                              unsigned int * pCount);



/** 
 CPMIIterObjIsEmpty checks if there are any objects in the query result.
 @param  hIter [IN] The handle to the object iterator.
 @return CP_S_OK, CP_S_FALSE, CP_E_INVALIDARG */
CPMI_C_API 
cpresult CPMIIterObjIsEmpty (HCPMIITEROBJ hIter);


/** 
  CPMIIterObjIsDone tests if the iteration has been completed.
  @param  hIter [IN]  The handle to the object iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterObjIsDone (HCPMIITEROBJ hIter);


/**
 CPMIIterObjRestart starts the iteration over from the beginning. This function can be
 called anytime during the iteration.
 @param  hIter [IN]  The handle to the object iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterObjRestart (HCPMIITEROBJ hIter);


/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                     Field Iteration APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMIIterFldGetNext retrieves a handle to the next field in the class, if this is the first call
 to the function, the first field handle is retrieved.
 @param  hIter [IN]  The handle to the field iterator.
 @param  phFld [OUT] The address of the HCPMIFLD variable to be set to the field
					 handle.On failure, *phFld is set to NULL. *phFld should be released
					 when no longer needed.
 @return CP_S_OK, CP_E_ITER_NOMORE, CP_E_INVALIDARG */
CPMI_C_API 
cpresult CPMIIterFldGetNext (HCPMIITERFLD   hIter,
                             HCPMIFLD     * phFld);


/** 
  CPMIIterFldGetCount retrieves the number of fields in the class.
  @param  hIter  [IN]  The handle to the field iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of fields. On failure, *pCount is set to ((unsigned int)-1).
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterFldGetCount (HCPMIITERFLD   hIter,
                              unsigned int * pCount);


/** 
  CPMIIterFldIsEmpty checks if the iteration object is empty.
  @param  hIter [IN] hThe handle to the field iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterFldIsEmpty (HCPMIITERFLD hIter);


/** 
  CPMIIterFldIsDone tests whether the iteration has been completed. 
  @param  hIter [IN] The handle to the field iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterFldIsDone (HCPMIITERFLD hIter);


/** 
  CPMIIterFldRestart starts the iteration over from the beginning. This function can be
  called anytime during the iteration.
  @param  hIter [IN] The handle to the field iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterFldRestart (HCPMIITERFLD hIter);


/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Schema Classes Iteration API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
  CPMIIterClassGetNext retrieves a handle to the next class in the schema, if this is the
  first call to the function, the first class handle is retrieved. 
  Return CP_E_ITERNOMORE when done. 
  @param  hIter   [IN]  The handle to the class iterator.
  @param  phClass [OUT] The address of the HCPMICLASS variable to be set to the class handle.
  						On failure, *phClass is set to NULL. *phClass should be released
  						when no longer needed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterClassGetNext (HCPMIITERCLASS   hIter, 
                               HCPMICLASS     * phClass);


/** 
  CPMIIterClassGetCount retrieves the number of classes in the schema.
  @param  hIter  [IN]  The handle to the class iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the number
					   of classes. On failure, *pCount is set to ((unsigned int)-1).
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterClassGetCount (HCPMIITERCLASS   hIter, 
                                unsigned int   * pCount);


/** 
 CPMIIterClassIsEmpty checks if the schema has no classes. 
 @param  hIter [IN] The handle to the class iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterClassIsEmpty (HCPMIITERCLASS hIter);


/** 
  CPMIIterClassIsDone tests whether the iteration has been completed.
  @param  hIter [IN] The handle to the class iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterClassIsDone (HCPMIITERCLASS hIter);


/** 
 CPMIIterClassRestart starts the iteration over from the beginning. This function can be
 called anytime during the iteration.
 @param  hIter [IN] The handle to the class iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterClassRestart (HCPMIITERCLASS hIter);


/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Containers Iteration API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMIIterCntrGetNext retrieves the next element in the container.
 Return CP_E_ITERNOMORE when done. 
 @param  hIter  [IN]  The handle to the container iterator.
 @param  pValue [OUT] Address of the tCPMI_FIELD_VALUE variable to be set to the
					  element. On failure, pValue->fvt is set to eCPMI_FVT_UNDEFINED.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterCntrGetNext (HCPMIITERCNTR       hIter, 
                              tCPMI_FIELD_VALUE * pValue);


/** 
  CPMIIterCntrGetCount retrieves the number of elements in the container iteration. 
  @param  hIter  [IN]  The handle to the container iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of elements. On failure, *pCount is set to 
					   ((unsigned int)-1).
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterCntrGetCount (HCPMIITERCNTR   hIter, 
                               unsigned int  * pCount);


/** 
 CPMIIterCntrIsEmpty checks if the container iterator is empty. 
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterCntrIsEmpty (HCPMIITERCNTR hIter);


/** 
 CPMIIterCntrIsDone tests whether the iteration has been completed.
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterCntrIsDone (HCPMIITERCNTR hIter);


/** 
 CPMIIterCntrRestart starts the iteration over from the beginning. This function can be
 called anytime during the iteration.
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterCntrRestart (HCPMIITERCNTR hIter);


/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                Ordered Containers Iteration API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
 CPMIIterOrdCntrGetNext retrieves the next element in the ordered container.
 Return CP_E_ITERNOMORE when done. 
 @param  hIter  [IN]  The handle to the container iterator.
 @param  pValue [OUT] Address of the tCPMI_FIELD_VALUE variable to be set to the element.
 					  On failure, pValue->fvt is set to eCPMI_FVT_UNDEFINED.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterOrdCntrGetNext (HCPMIITERORDCNTR       hIter, 
                                 tCPMI_FIELD_VALUE * pValue);


/** 
  CPMIIterOrdCntrGetCount retrieves the number of elements in the ordered container
  iterator. 
  @param  hIter  [IN]  The handle to the container iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of elements. On failure, *pCount is set to ((unsigned
					   int)-1).  
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterOrdCntrGetCount (HCPMIITERORDCNTR   hIter, 
                                  unsigned int     * pCount);


/** 
  CPMIIterOrdCntrIsEmpty checks if the ordered container iterator is empty. 
  @param  hIter [IN] The handle to the container iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterOrdCntrIsEmpty (HCPMIITERORDCNTR hIter);


/** 
  CPMIIterOrdCntrIsDone tests whether the iteration has been completed. 
  @param  hIter [IN] handle to iteration object
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterOrdCntrIsDone (HCPMIITERORDCNTR hIter);


/** 
  CPMIIterOrdCntrRestart starts the iteration over from the beginning. This function can
  be called anytime during the iteration.
  @param  hIter [IN] The handle to the container iterator.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterOrdCntrRestart (HCPMIITERORDCNTR hIter);


/*@}*/

/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                    Context Iteration API
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMIIterContextGetNext retrieves the next field in the context.
 Return CP_E_ITERNOMORE when done. 
 @param  hIter  [IN]  The handle to the context iterator.
  @param  psz  [OUT]  The address of a string variable which receives the next field value.
					  On failure, *psz is set to NULL. *psz should not be freed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterContextGetNext (HCPMIITERCONTEXT       hIter, 
                              const char ** psz);


/** 
  CPMIIterContextGetCount retrieves the number of elements in the container iteration. 
  @param  hIter  [IN]  The handle to the container iterator.
  @param  pCount [OUT] The address of the unsigned int variable to be set to the
					   number of elements. On failure, *pCount is set to 
					   ((unsigned int)-1).
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterContextGetCount (HCPMIITERCONTEXT   hIter, 
                               unsigned int  * pCount);


/** 
 CPMIIterContextIsEmpty checks if the container iterator is empty. 
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterContextIsEmpty (HCPMIITERCONTEXT hIter);


/** 
 CPMIIterContextIsDone tests whether the iteration has been completed.
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterContextIsDone (HCPMIITERCONTEXT hIter);


/** 
 CPMIIterContextRestart starts the iteration over from the beginning. This function can be
 called anytime during the iteration.
 @param  hIter [IN] The handle to the container iterator.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIIterContextRestart (HCPMIITERCONTEXT hIter);


/*@}*/




/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Reference APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
 CPMIRefCreateFromName creates a reference to an object based on the name and table
 name of the object.
 @param  szTblName [IN]  The table name.
 @param  szObjName [IN]  The object name.
 @param  phRef     [OUT] The address of the HCPMIREF variable which receives the reference
						 handle. On failure, *phRef is set to NULL. *phRef should be released
						 when no longer needed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIRefCreateFromName (const char * szTblName, 
                                const char * szObjName, 
                                HCPMIREF   * phRef); 


/** 
 CPMIRefCreateFromObj creates a reference to an object based on the object handle.
 Release with CPMIHandleRelease when not needed anymore. 
 @param  hObj      [IN]  The object handle.
 @param  phRef     [OUT] The address of the HCPMIREF variable which receives the reference
						 handle. On failure, *phRef is set to NULL. *phRef should be released
						 when no longer needed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIRefCreateFromObj (HCPMIOBJ   hObj,
                               HCPMIREF * phRef);


/** 
  CPMIRefGetReferencedObj retrieves the referenced object.
  @param  hRef     [IN]  The reference handle.
  @param  hDb      [IN]  The handle to the database containing the object.
  @param  pfnCB    [IN]  The function to be called when the object has been dereferenced.
  @param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
  @param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
						 *pOpId is set to CPMIOPID_ERR upon failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIRefGetReferencedObj (HCPMIREF     hRef    , 
                                  HCPMIDB      hDb     , 
                                  CPMIObj_CB   pfnCB   , 
                                  void       * pvOpaque, 
                                  cpmiopid   * pOpId);


/** 
 CPMIRefGetObjectName retrieves the referenced object name.
 @param  hRef [IN]  The reference handle.
 @param  psz  [OUT] The address of a string variable which receives the object name. On
					failure, *psz is set to NULL. *psz should not be freed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API
cpresult CPMIRefGetObjectName (HCPMIREF      hRef, 
                               const char ** psz);


/** 
  CPMIRefGetTableName retrieves the referenced object table name.
  @param  hRef [IN]  The reference handle.
  @param  psz  [OUT] The address of a string variable which receives the object name.
					 On failure, *psz is set to NULL. *psz should not be freed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API
cpresult CPMIRefGetTableName (HCPMIREF      hRef, 
                              const char ** psz);


/** 
  CPMIRefSetContext sets a context definition for the reference. The Context will be used
  to reduce the object once the reference is de-referenced.
  @param  hRef     [IN] The reference handle.
  @param  hContext [IN] The Context handle.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API
cpresult CPMIRefSetContext (HCPMIREF     hRef, 
                            HCPMICONTEXT hContext);

/*@}*/



/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Notifications APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMINotifyGetEvent retrieves the type of event that triggered the notification.
 @param  hMsg   [IN]  The notification message handle.
 @param  pEvent [OUT] The address of tCPMI_NOTIFY_EVENT variable which receives the
					  notification event type.
                      *pEvent is set to ((unsigned int)-1) upon failure.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetEvent (HCPMINOTIFYMSG       hMsg, 
                             tCPMI_NOTIFY_EVENT * pEvent); 


/** 
  CPMINotifyGetTblName retrieves the name of the table containing the object that
  triggered the event.
  @param  hMsg [IN]  The notification message handle.
  @param  psz  [OUT] The address of a string variable which receives the table name. On
					 failure, *psz is set to NULL.*psz should not be freed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetTblName (HCPMINOTIFYMSG    hMsg, 
                               const char     ** psz);


/** 
 CPMINotifyGetObjName retrieves the name of the object that triggered the event.
 @param  hMsg [IN]  The notification message handle.
 @param  psz  [OUT] The address of string variable which receives the object name. On
					failure, *psz is set to NULL.*psz should not be freed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetObjName (HCPMINOTIFYMSG    hMsg, 
                               const char     ** psz);



/** 
CPMINotifyGetOldName  gets the old name of the object that triggered the notification.  
@param  hMsg [IN]  The notification message handle.
@param  psz  [OUT] The address of a the constant char* variable to
				   receive the old name.
@return CP_S_OK, CP_E_INVALIDARG */
CPMI_C_API 
cpresult CPMINotifyGetOldName (HCPMINOTIFYMSG    hMsg, 
                               const char     ** psz);


/** 
  CPMINotifyGetObj retrieves the handle of the object that triggered the event.
  The returned bject handle will be valid only if the flag
  eCPMI_FLAG_GET_SELF_CHANGES was used in the registration stage or the noification is 
  about status changes i.e.b notification type is eCPMI_NOTIFY_STATUS_CHANGE.
  @param  hMsg  [IN]  TThe notification message handle.
  @param  phObj [OUT] The address of the HCPMIOBJ which receives the object handle. On
					  failure, *phObj is set to NULL. *phObj should be released when no
					  longer needed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetObj (HCPMINOTIFYMSG   hMsg,
                           HCPMIOBJ       * phObj);


/** 
  CPMINotifyGetUid retrieves the uid of the object that triggered the notification.
  @param  hMsg [IN]  The notification message handle.
  @param  psz  [OUT] The address of a string variable which receives the UID string. On
					 failure, *psz is set to NULL. *psz should not be freed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetUid (HCPMINOTIFYMSG    hMsg, 
                           const char     ** psz);


/** 
  CPMINotifyGetModifierUser retrieves the name of the administrator whose modification
  triggered the event.
  @param  hMsg [IN]  The notification message handle.
  @param  psz  [OUT] The address of a string variable which receives the administrator
					 name. On failure, *psz is set to NULL. *psz should not be freed.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetModifierUser (HCPMINOTIFYMSG    hMsg, 
                                    const char     ** psz);

/** 
 CPMINotifyGetModifierHost retrieves the name of the host from which the event was
 triggered.
 @param  hMsg [IN]  The notification message handle.
 @param  psz  [OUT] The address of a string variable which receives the host name. On
					failure, *psz is set to NULL. *psz should not be freed
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetModifierHost (HCPMINOTIFYMSG    hMsg, 
                                    const char     ** psz);


/** 
 CPMINotifyGetTime retrieves the time the event was triggered.
 @param  hMsg  [IN]  The notification message handle.
 @param  pTime [OUT] The address of a time_t variable which receives the time of event.
					 On failure, *pTime is set to ((time_t)0).
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMINotifyGetTime (HCPMINOTIFYMSG   hMsg, 
                            time_t         * pTime);

                                         

/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/**
 * 
 * @name                      Application Object APIs 
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
 CPMIAppGetObject retrieves a handle to the underlying object. 
 @param  hApp  [IN]  The application handle.
 @param  phObj [OUT] The address of the HCPMIOBJ variable to be set to the object handle.
 					 On failure, *phObj is set to NULL. *phObj should be released when
					 no longer needed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppGetObject (HCPMIAPP   hApp, 
                           HCPMIOBJ * phObj);


/** 
  CPMIAppCreateCertificate enables the creation of a new certificate. This function
  initialized the SIC module by changing its connection state from uninitialized to
  initialized. If the operation succeeds the 'eState' parameter of pfnCB is set to
  eCPMI_CERT_STATE_INITIALIZED and the 'stat' parameter will be CP_S_OK. Otherwise the
  'eState' will be eCPMI_CERT_STATE_UNINITIALIZED and the 'stat' parameter will indicates
  the reason for failure.
  @param  hApp     [IN]  The application handle.
  @param  szPasswd [IN]  The password required by the Internal Certificate Authority.
  @param  pfnCB    [IN]  The function to be called once the certificate has been created. 
  @param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
  @param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
						 *pOpId is set to CPMIOPID_ERR upon failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppCreateCertificate (HCPMIAPP             hApp    ,
                                   const char         * szPasswd,
                                   CPMICertificate_CB   pfnCB   ,
                                   void               * pvOpaque,
                                   cpmiopid           * pOpId);



/** 
CPMIAppPushCertificate pushes a certificate that has already been created. 
This function changes the connection state of the SIC module from initialized 
to communicating. If the operation succeeds the 'eState' parameter of pfnCB 
will be set to eCPMI_CERT_STATE_PUSHED and the 'stat' parameter will be CP_S_OK. 
Otherwise the 'eState' will be eCPMI_CERT_STATE_INITIALIZED and the 'stat' 
param will indicate the reason for failure.
@param  hApp     [IN]  The application handle.
@param  pfnCB    [IN]  The function to be called once the certificate has been pushed.
@param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
                       *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppPushCertificate (HCPMIAPP             hApp    ,
                                 CPMICertificate_CB   pfnCB   ,
                                 void               * pvOpaque,
                                 cpmiopid           * pOpId);



/** 
CPMIAppRevokeCertificate enables revoking an already created or pushed certificate.
The purpose of this API is to change the module connection state from initiated to
unitialized. If the operation succeeds the 'eState' parameter of pfnCB will be
set to eCPMI_CERT_STATE_UNINITIALIZED and the 'stat' parameter will be CP_S_OK.
Otherwise the 'eState' will be eCPMI_CERT_STATE_PUSHED and the 'stat' parameter will
indicate the reason for failure.
@param  hApp     [IN]  The application handle.
@param  pfnCB    [IN]  The function to be called once the certificate has been revoked.
@param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
@param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
                       *pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppRevokeCertificate (HCPMIAPP             hApp    , 
                                   CPMICertificate_CB   pfnCB   , 
                                   void               * pvOpaque, 
                                   cpmiopid           * pOpId);



/** 
 CPMIAppAttachLicense associates a license object from the License Repository with a
 given application object.
 Where in the callback function (pfnAttachCB) the user gets indication for 
 successful request with the or failure and the reason for the failure.
 @param  HCPMIAPP [IN]  The application handle.
 @param  hObj     [IN]  Handle to license object.
 @param  pfnCB    [IN]  User function to be called when operation is done.
 @param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
 @param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
						*pOpId is set to CPMIOPID_ERR upon failure.
@return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppAttachLicense  (HCPMIAPP             hApp    ,
                                HCPMIOBJ			 hObj,
                                CPMILicense_CB        pfnCB   ,
                                void               * pvOpaque,
                                cpmiopid           * pOpId);



/** 
 CPMIAppDetachLicense detaches a license object from the License Repository from a
 given application object.
 Where in the callback function (pfnAttachCB) the user gets indication for 
 successful request with the or failure and the reason for the failure.
 @param  hApp     [IN]  The application handle.
 @param  hObj     [IN]  Handle to license object. 
 @param  pfnCB    [IN]  UUser function to be called when operation is done.
 @param  pvOpaque [IN]  User-supplied data to be passed to the callback function.
 @param  pOpId    [OUT] The address of the cpmiopid variable to receive the operation id.
       					*pOpId is set to CPMIOPID_ERR upon failure.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIAppDetachLicense  (HCPMIAPP             hApp,
                                HCPMIOBJ             hObj,
                                CPMILicense_CB       pfnCB,
                                void               * pvOpaque,
                                cpmiopid           * pOpId);



/** 
  CPMIAppsInstallPolicy installs a policy on an array of an application object.
  The user function will be called
  separately for every application object from the application array reporting its state and
  status..
  @param  pApps             [IN]  Array of application objects to install on.
  @param  nAppsCount        [IN]  Number of applications in pApps (size of pApps).
  @param  szPolicyName      [IN]  The name of the policy object.
  @param  nInstallOptions   [IN]  OR'ed combination from tCPMI_POLICY_INSTALL_OPTION.
  @param  nInstallOptionsEx [IN]  For Future use. Currently being ignored. 
  @param  pfnCB             [IN]  User function to be called when operation is done.
  @param  pvOpaque          [IN]  User-supplied data to be passed to the callback function.
  @param  pOpId             [OUT] Address of cpmiopid variable to receive the 
                                  operation id. *pOpId is set to CPMIOPID_ERR upon failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult
CPMIAppsInstallPolicy (HCPMIAPP* pApps,
                       unsigned int nAppsCount,
                       const char* szPolicyName,
                       unsigned int nInstallOptions,
                       unsigned int nInstallOptionsEx,
                       CPMIPolicyInstall_CB pfnCB,
                       void* pvOpq,
                       cpmiopid* pOpid);



/** 
  CPMIAppsUnInstallPolicy uninstall a policy from an array of application objects.
  The user function will be called separately for every application object from the application array
  reporting its state and status.
  @param  pApps             [IN]  Array of application objects to uninstall from.
  @param  nAppsCount        [IN]  Number of applications in pApps (size of pApps).
  @param  nInstallOptions   [IN]  OR'ed combination from tCPMI_POLICY_INSTALL_OPTION.
  @param  nInstallOptionsEx [IN]  For Future use. Currently being ignored. 
  @param  pfnCB             [IN]  User function to be called when operation is done.
  @param  pvOpaque          [IN]  User data to pass to pfnCB.
  @param  pOpId             [OUT] Address of cpmiopid variable to receive the 
                         		  operation id. *pOpId is set to CPMIOPID_ERR upon failure.
  @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult
CPMIAppsUnInstallPolicy (HCPMIAPP* pApps,
                         unsigned int appsCount,
                         unsigned int nUnInstallOptions,
                         unsigned int nUnInstallOptionsEx,
                         CPMIPolicyInstall_CB pfnCB, 
                         void * pvOpq, 
                         cpmiopid * pOpid);


/*@}*/

/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Context APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
  CPMIContextCreate creates a Context Object. 
  @param  szContextType [IN] The type of context to create. The supported type is
							 dynamic_context.
  @param  phContext 	[OUT] The address of the HCPMICONTEXT receiving the result.
  @return CP_S_OK, CP_E_FAIL, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIContextCreate (const char* szContextType, HCPMICONTEXT* phContext); 

/** 
 CPMIContextAddField add a field name to the Context Object. 
 @param  hContext    [IN] The Context handle.
 @param  szFieldName [IN] The field name.
 @return CP_S_OK */
CPMI_C_API 
cpresult CPMIContextAddField (HCPMICONTEXT hContext, const char* szFieldName); 

/** 
 CPMIContextRemoveField removes a field name from the Context Object. 
 @param  hContext    [IN] The Context handle.
 @param  szFieldName [IN] The field name.
 @return CP_S_OK, CP_S_FALSE (if field doesn't exist) */
CPMI_C_API 
cpresult CPMIContextRemoveField (HCPMICONTEXT hContext, const char* szFieldName); 

/** 
CPMICntrIterElements retrieves a handle to an element iterator object. 
@param  hCnt   [IN]  The container handle.
@param  phIter [OUT] The address of the HCPMIITERCNTR variable to be set to 
                     the element iterator handle. On failure, *phIter is set to NULL. 
                     *phIter should be released when done.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_HANDLE. */
CPMI_C_API 
cpresult CPMIContextIterElements (HCPMICONTEXT  hContext , 
                               HCPMIITERCONTEXT * phIter);

/*@}*/

/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                        Result APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
 CPMIResultIterObj gets an iteration handle to iterate over the objects on a given query
 result. Release the HCPMIITEROBJ var iable with CPMIHandleRelease when not
 needed anymore.
 @param  hRes   [IN]  A handle to the query results.
 @param  phIter [OUT] The address of the HCPMIITEROBJ variable to be set to the iterator
					  handle. On failure, *phIter is set to NULL.*phIter should be
					  released when no longer needed.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIResultIterObj (HCPMIRSLT      hRes, 
                            HCPMIITEROBJ * phIter);

/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/** 
 *
 * @name                       Unique Id APIs
 *
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/** 
 CPMIUidAreEqual compare two UIDs for exact matches. 
 @param  pUid1 [IN] Pointer to the first UID.
 @param  pUid2 [IN] Pointer to the second UID.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIUidAreEqual (const tCPMI_UID * pUid1, 
                          const tCPMI_UID * pUid2);


/** 
 CPMIUidToString convert a UID into an allocated string in the format:
 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. 
 @param  pUid   [IN]  The object ID.
 @param  pszUid [OUT] Returned uid as string. On failure, *pszUid is set to NULL. *pszUid
					  should be freed using CPMIFreeString.
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIUidToString (const tCPMI_UID *  pUid, 
                          char            ** psz); 


/** 
 CPMIUidFromString converts a string in the format:
 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx 
 @param  szUid   [IN]  A string representing the object UID.
 @param  pUid    [OUT] The address of a tCPMI_UID variable which receives the converted
                       UID. *pUid is set to a NULL UID on failure (a UID full of zeros).
 @return CP_S_OK, CP_E_INVALIDARG, and case specific error codes */
CPMI_C_API 
cpresult CPMIUidFromString (const char * szUid, 
                            tCPMI_UID  * pUid);



/*@}*/


/* ////////////////////////////////////////////////////////////////////////// */
/**
 * 
 * @name                     Menory Management APIs 
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */

/** 
CPMIFreeString frees strings allocated by various CPMI API functions.
@param sz [IN] A pointer to the string to be freed.  */
CPMI_C_API 
void CPMIFreeString (char * sz);


/** 
  CPMIHandleAddRef increments the handle reference count. Use it to explicitly increment
  the reference count of the handle in order to make sure the handle remains valid.
  @param  handle [INOUT] The handle to increment.
  @return new reference count - for debug purposes only  */
CPMI_C_API 
unsigned long CPMIHandleAddRef (HCPMI handle); 


/** 
  CPMIHandleRelease decrements the handle reference count. Use this function to
  release holding of CPMI handle that was returned by some CPMI API, or a handle
  passed to CPMIHandleAddRef.
  For safety, after call to CPMIHandleRelease, assign the handle to NULL.
  @param  handle [INOUT] The handle to decrement.
  @return new reference count - for debug purposes only  */
CPMI_C_API 
unsigned long CPMIHandleRelease (HCPMI handle); 


/** 
 CPMIReleaseFieldValue releases the string or field handle (according to the fvt
 member) in the designated tCPMI_FIELD_VALUE. 
 @param pFv [IN] The structure whose field is to be released. */
CPMI_C_API 
void CPMIReleaseFieldValue (tCPMI_FIELD_VALUE * pFv);


/*@}*/



/* ////////////////////////////////////////////////////////////////////////// */
/**
 * 
 * @name                     Utility APIs 
 */
/*@{*/
/* ////////////////////////////////////////////////////////////////////////// */


/**
CPMIGetMajorReleaseVer retreives the CPMI client library major version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pMajorVer [INOUT] The address of an unsigned int variable that receives
						 the major release version.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMIGetMajorReleaseVer (unsigned int * pMajorVer);



/**
CPMIGetMinorReleaseVer retreives the CPMI client library minor version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pMinorVer [INOUT] The address of an unsigned int variable that receives
						 the minor (feature pack) release version.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMIGetMinorReleaseVer (unsigned int * pMinorVer);



/**
CPMIGetServicePackVer retreives the CPMI client library service pack version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pSPVer [INOUT] The address of an unsigned int variable that receives
                      the service pack release version.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMIGetServicePackVer (unsigned int * pSPVer);



/**
CPMIGetHotFixVer retreives the CPMI client library hotfix version number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pHFVer [INOUT] The address of an unsigned int variable that receives
					  the hotfix version.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMIGetHotFixVer (unsigned int * pHFVer);



/**
CPMIGetBuildVer retreives the CPMI client library build number.
The version retrieval functions CPMISessionGetServerMajorVersion, 
CPMISessionGetServerMinorVersion, CPMISessionGetServerSPVersion, 
CPMISessionGetServerHFVersion, CPMISessionGetServerBuildNumber,
CPMIGetMajorReleaseVer, CPMIGetMinorReleaseVer, CPMIGetServicePackVer,
CPMIGetHotFixVer and CPMIGetBuildVer can be used to determine if the 
application will run properly with current CPMI client and server versions.
@param pBuildNum [INOUT] The address of an unsigned int variable that receives
                         the build number.
@return CP_S_OK, CP_E_INVALIDARG  */
CPMI_C_API 
cpresult CPMIGetBuildVer (unsigned int * pBuildNum);



/**
CPMICrypt encrypt strings in a way that can be later authenticated by FireWall-1. 
Use this API to encrypt password-like fields, for example user passwords.
@param szPasswd     [IN] string variable containing the password to encrypt. 
@param pszEncPasswd [inout] address of a string variable to receive the encryption
                            result of szPasswd. *pszEncPasswd should be released 
                            with CPMIFreeString.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_FAIL */
CPMI_C_API 
cpresult CPMICrypt (const char * szPasswd, char ** pszEncPasswd);



/**
CPMIDbSetSessionDescription changes the client session description. After calling
this function each audit will contain the description in the "Session ID" field.
@param  hDb [IN] a valid CPMI database handle.
@param  szSessionDesc	 [IN]  string variable containing the new session description.                         
@param  pfnCB    [IN]  User function to be called when operation is done.
@param  pvOpaque [IN]  User data to pass to pfnCB.
@param  pOpid    [OUT] Address of the cpmiopid variable to be set to the 
                       operation ID. On failure, *pOpId is set to CPMIOPID_ERR.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_FAIL */
CPMI_C_API 
cpresult CPMIDbSetSessionDescription(
	HCPMIDB		hDb,
	const char	* szSessionDesc,
	CPMIDb_CB	pfnCb,
	void		* pvOpaque,
	cpmiopid	* pOpId);



/**
CPMIDbGetObjectDisplayInfo retrieves relative paths to icons of database objects.
@param  hDb            [IN]  a valid CPMI database handle.
@param  pRequests      [IN]  array containing the requests for display information.    
@param  nRequestsCount [IN]  Number of requests for display information.                     
@param  pfnCB          [IN]  User function to be called when operation is done.
@param  pvOpaque       [IN]  User data to pass to pfnCB.
@param  pOpid          [OUT] Address of the cpmiopid variable to be set to the 
                             operation ID. On failure, *pOpId is set to CPMIOPID_ERR.
@return CP_S_OK, CP_E_INVALIDARG, CP_E_FAIL */
CPMI_C_API 
cpresult CPMIDbGetObjectDisplayInfo(
	HCPMIDB		hDb,
	tCPMI_OBJECT_DISPLAY_INFO_REQUEST * pRequests,
	unsigned int nRequestsCount,
	CPMIObjDispInfo_CB	pfnCb,
	void		* pvOpaque,
	cpmiopid	* pOpId);


/*@}*/


#endif /* CPMICLIENTAPIS_H_EB2C4486CC8611D2A3E30090272CCB30 */

/* //////////////////////// EOF CPMIClientAPIs.h //////////////////////// */


