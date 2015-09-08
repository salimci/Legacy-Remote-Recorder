#ifndef __LEA_FILTER_EXT_H
#define __LEA_FILTER_EXT_H

#include "opsec/opsec.h"
#include "opsec/opsec_export_api.h"
#include "opsec/lea.h"
#include "opsec/lea_filter.h"

#ifdef	__cplusplus
extern "C" {
#endif


/*
 * Client-side filtering function prototypes
 */
 
DLLIMP int
lea_filter_rulebase_register_local(OpsecSession *session, LeaFilterRulebase *filter);

DLLIMP int
lea_filter_rulebase_unregister_local(OpsecSession *session);


#ifdef __cplusplus
}
#endif

#endif /* !__LEA_FILTER_EXT_H */

