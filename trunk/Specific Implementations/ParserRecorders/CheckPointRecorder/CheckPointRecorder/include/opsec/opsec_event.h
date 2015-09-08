/*
 * OPSEC opsec_event include file for 3rd party use.
 */
#ifndef _opsec_event_h_
#define _opsec_event_h_

#include <opsec/opsec.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef int (*OpsecEventHandler) (OpsecEnv * env, int event_no, 
				  void *raise_data, void *set_data);

DLLIMP int opsec_new_event_id ();

DLLIMP int opsec_set_event_handler (OpsecEnv *env, int event_no, 
				     OpsecEventHandler handler, void *set_data);
DLLIMP int opsec_del_event_handler (OpsecEnv *env, int event_no, 
				     OpsecEventHandler handler, void *set_data);

DLLIMP int opsec_suspend_event_handler (OpsecEnv *env, int event_no, 
				OpsecEventHandler handler, void *set_data);
DLLIMP int opsec_resume_event_handler (OpsecEnv *env, int event_no, 
				OpsecEventHandler handler, void *set_data);

DLLIMP int opsec_raise_event (OpsecEnv *env, int event_no, void *raise_data);
DLLIMP int opsec_raise_persistent_event (OpsecEnv *env,  int event_no, void *raise_data);
DLLIMP int opsec_unraise_event (OpsecEnv *env,  int event_no, void *raise_data);
DLLIMP int opsec_israised_event (OpsecEnv *env, int event_no, void *raise_data);

#ifdef __cplusplus
}
#endif

#endif  /* _opsec_event_h_ */
