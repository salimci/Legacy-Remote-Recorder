#ifndef __SHARED_LIB_SERVER_H__
#define __SHARED_LIB_SERVER_H__

#include "opsec/opsec_export_api.h"
#include <opsec/opsec.h>
#include <opsec/opsec_error.h>


#ifdef __cplusplus
extern "C" {
#endif


/*
 * Shred library and server handles
 */
typedef struct _SL_handle        SL_handle;
typedef struct _SL_ServerHandle  SL_ServerHandle;
typedef void * (*SL_Proc)();


/*
 * Shared library Load/Un-Load
 */
 
 /* -------------------------------------------------------------------------
  |  opsec_sl_load:
  |  --------------
  |  
  |  Description:
  |  ------------
  |  Loads a shared library.
  |
  |  Parameters:
  |  -----------
  |  lib_name - name of the library (in some cases, including path).
  |
  |  Returned value:
  |  ---------------
  |  Pointer to shared library handle, if successful, NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP SL_handle *opsec_sl_load(char *lib_name);

 /* -------------------------------------------------------------------------
  |  opsec_sl_unload:
  |  ----------------
  |  
  |  Description:
  |  ------------
  |  Un-Loads a shared library.
  |
  |  Parameters:
  |  -----------
  |  sl_handle - returned from a call to 'opsec_sl_load'.
  |
  |  Returned value:
  |  ---------------
  |  Zero, if successful, -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int opsec_sl_unload(SL_handle *sl_handle);

/*
 * Shared-library-server handling API
 */

 /* -------------------------------------------------------------------------
  |  opsec_sl_init_server:
  |  ---------------------
  |  
  |  Description:
  |  ------------
  |  Initializes a shared library server.
  |
  |  Parameters:
  |  -----------
  |  sl_handle - returned from a call to 'opsec_sl_load'.
  |  env       - on which the server will run.
  |  srv_name  - Name of the server (as determined by the shared lib).
  |
  |  Returned value:
  |  ---------------
  |  Pointer shared library server handle, if successful, NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP SL_ServerHandle *opsec_sl_init_server(SL_handle *sl_handle,
                                               OpsecEnv  *env,
                                               char      *srv_name);

 /* -------------------------------------------------------------------------
  |  opsec_sl_destroy_server:
  |  ------------------------
  |  
  |  Description:
  |  ------------
  |  Destroys a shared library server.
  |
  |  Parameters:
  |  -----------
  |  s_handle - returned from a call to 'opsec_sl_init_server'.
  |
  |  Returned value:
  |  ---------------
  |  Zero, if successful, -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int opsec_sl_destroy_server(SL_ServerHandle *s_handle);

 /* -------------------------------------------------------------------------
  |  opsec_sl_start_server:
  |  ----------------------
  |
  |  Description:
  |  ------------
  |  Starts a shared library server.
  |
  |  Parameters:
  |  -----------
  |  s_handle - returned from a call to 'opsec_sl_init_server'.
  |
  |  Returned value:
  |  ---------------
  |  Pointer to 'OpsecEntity' structure, if successful, NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP OpsecEntity *opsec_sl_start_server(SL_ServerHandle *s_handle);

 /* -------------------------------------------------------------------------
  |  opsec_sl_stop_server:
  |  ----------------------
  |  
  |  Description:
  |  ------------
  |  Stops a shared library server.
  |
  |  Parameters:
  |  -----------
  |  s_handle - returned from a call to 'opsec_sl_init_server'.
  |
  |  Returned value:
  |  ---------------
  |  Zero, if successful, -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int opsec_sl_stop_server(SL_ServerHandle *s_handle);

 /* -------------------------------------------------------------------------
  |  1. opsec_sl_get_server_name
  |  2. opsec_sl_get_server_entity
  |  3. opsec_sl_is_server_running
  |  -----------------------------
  |  
  |  Description:
  |  ------------
  |  Shared library server handle access API.
  |
  |  Parameters:
  |  -----------
  |  s_handle - returned from a call to 'opsec_sl_init_server'.
  |
  |  Returned value:
  |  ---------------
  |  Shared library server name, OpsecEntity, & is_running.
   ------------------------------------------------------------------------*/

  DLLIMP char        *opsec_sl_get_server_name  (SL_ServerHandle *s_handle);
  DLLIMP OpsecEntity *opsec_sl_get_server_entity(SL_ServerHandle *s_handle);
  DLLIMP int          opsec_sl_is_server_running(SL_ServerHandle *s_handle);

/* -------------------------------------------------------------------------
  |  opsec_sl_get_proc
  |  -----------------
  |  
  |  Description:
  |  ------------
  |  Shared library server handle access API.
  |
  |  Parameters:
  |  -----------
  |  sl_handle - returned from a call to 'opsec_sl_init_server'.
  |  proc_name - name of the function
  |
  |  Returned value:
  |  ---------------
  |  return a pointer to the function in that shared library.
   ------------------------------------------------------------------------*/
  DLLIMP SL_Proc opsec_sl_get_proc(SL_handle *sl_handle, const char *proc_name);


/*
 * The shared library will define (export) the following API:
 */
#define OPSEC_SL_EXP_INIT             "opsec_sl_exp_init_sl"              /* Optional */
#define OPSEC_SL_EXP_CLOSE            "opsec_sl_exp_close_sl"             /* Optional */
#define OPSEC_SL_EXP_INIT_SRV_ENT     "opsec_sl_exp_init_server_entity"
#define OPSEC_SL_EXP_DESTROY_SRV_ENT  "opsec_sl_exp_destroy_server_entity"
#define OPSEC_SL_EXP_START_SRV        "opsec_sl_exp_start_server"
#define OPSEC_SL_EXP_STOP_SRV         "opsec_sl_exp_stop_server"


/*

  Matching headers for the above functions:

  OPSEC_SL_EXP_INIT            -  int opsec_sl_exp_init_sl(void)
  OPSEC_SL_EXP_CLOSE           -  int opsec_sl_exp_close_sl(void)
  OPSEC_SL_EXP_INIT_SRV_ENT    -  OpsecEntity *opsec_sl_exp_init_server_entity(char *srv_name, OpsecEnv *env);
  OPSEC_SL_EXP_DESTROY_SRV_ENT -  void opsec_sl_exp_destroy_server_entity(OpsecEntity *server);
  OPSEC_SL_EXP_START_SRV       -  OpsecEntity *opsec_sl_exp_start_server(char *srv_name);
  OPSEC_SL_EXP_STOP_SRV        -  int opsec_sl_exp_stop_server(char *srv_name);

*/


#ifdef __cplusplus
}
#endif

#endif /* __SHARED_LIB_SERVER_H__ */

