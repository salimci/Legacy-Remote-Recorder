/*
 * OPSEC include file for 3rd party use.
 */

#ifndef UFP_SERVER_H
#define UFP_SERVER_H

#include "opsec_export_api.h"

#include "opsec/ufp_opsec.h"
 
#ifdef __cplusplus
extern "C" {
#endif
 
#define MAX_UFP_MASK_SIZE 100	/* in bytes */

#define UFP_DESC_HANDLER _NEW_HANDLER_ATTR(OPSEC_UFP_SERVER, 0)
#define UFP_DICT_HANDLER _NEW_HANDLER_ATTR(OPSEC_UFP_SERVER, 1)
#define UFP_CAT_HANDLER	 _NEW_HANDLER_ATTR(OPSEC_UFP_SERVER, 2)

/*
 * UFP server replay API
 */
DLLIMP int ufp_send_desc_reply( OpsecSession *session, char *desc, int status);
DLLIMP int ufp_send_dict_reply( OpsecSession *session,
				char **dict,
				int dict_ver,
				int dict_elems,
				int ufp_mask_len,
				int status );
DLLIMP int ufp_send_cat_reply( OpsecSession *session, ufp_mask mask, int ufp_mask_len, int status);
DLLIMP int ufp_send_cat_reply_with_cache_info (OpsecSession *session,
						ufp_mask      mask,
						int           ufp_mask_len,
						int           status,
						UfpCacheInfo *cache_info,
						char         *redirect_url);
	
#ifdef __cplusplus
}
#endif
 
#endif /* UFP_SERVER_H */
