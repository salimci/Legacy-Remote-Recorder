/*
 * OPSEC include file for 3rd party use.
 */
 
#ifndef UFP_OPSEC_H
#define UFP_OPSEC_H


#include "opsec.h"

#ifdef __cplusplus
extern "C" {
#endif
	
typedef struct _UfpCacheInfo	UfpCacheInfo;
typedef struct _CacheEntry		CacheEntry;
typedef unsigned char *ufp_mask;
typedef char **ufp_dict;

/*
 * Mask setting API
 */
DLLIMP ufp_mask ufp_mask_init( int mask_len );
DLLIMP void     ufp_mask_destroy(ufp_mask mask);
DLLIMP void     ufp_mask_set(ufp_mask mask, int mask_len, int n);
DLLIMP void     ufp_mask_clr(ufp_mask mask, int mask_len, int n);
DLLIMP int      ufp_mask_isset(ufp_mask mask, int mask_len, int n);
DLLIMP ufp_mask ufp_mask_from_string(char *str_mask, int mask_len, ufp_mask mask);

/*
 * Client requests type
 */
typedef enum {	OPSEC_UFP_RQST_UNDEF=-1,
		OPSEC_UFP_RQST_DESC,
		OPSEC_UFP_RQST_DICT,
		OPSEC_UFP_RQST_CAT
} opsec_ufp_rqst_t ;

/*
 * UFP error types
 */
typedef enum { UFP_UNDEF = -1,
		UFP_OK,
		UFP_LENGTH_ERR,
		UFP_DICT_VER_ERR,
		UFP_GENERAL_ERR,
		UFP_PROTOCOL_NOT_SUPPORTED
} ufp_err_t;

/*
 * UFP mask types 
 */
typedef enum {	UNDEFINED_MASK = -1,
		RELATIVE_MASK,
		ABSOLUTE_MASK
} ufp_mask_type;

DLLIMP extern OpsecEntityType *UFP_SERVER ;

/*
 * UfpCacheInfo API
 */
DLLIMP UfpCacheInfo *ufp_create_cache_info(unsigned int ttl);
DLLIMP void ufp_destroy_cache_info(UfpCacheInfo *cache_info);
DLLIMP int ufp_add_to_cache_info (UfpCacheInfo *cache_info,
					char *locator,
					const char *ip,
					unsigned short  port,
					ufp_mask mask,			/* Cached mask */
					int mask_len,
					ufp_mask_type mask_type);	/* relative, absolute */

DLLIMP int ufp_get_ttl(UfpCacheInfo *cache_info);			
DLLIMP CacheEntry *ufp_get_first_cache_entry(UfpCacheInfo *cache_info);
DLLIMP CacheEntry *ufp_get_next_cache_entry(UfpCacheInfo *cache_info);

#ifdef __cplusplus
}
#endif
 

#endif /* UFP_OPSEC_H */
