#ifndef __OPSEC_VT_API_H
#define __OPSEC_VT_API_H

#include "opsec/opsec_export_api.h"

#if defined (__cplusplus)
extern "C" {
#endif


/*
 * 64 Bits Type Definition 
 */
#ifndef _64BIT 
#if defined(WIN32) && !defined(WU_APP)
#define _64BIT __int64
#else
#define _64BIT long long
#endif
#endif /* _64BIT */
 
typedef          _64BIT opsec_int64;
typedef unsigned _64BIT opsec_u_int64;

#if defined (__cplusplus)
}
#endif

/*
 * IPv6 Type (as defined in RFC 2133)
 */
#if defined(linux) || defined(aix) || defined(solaris2)

#include <netinet/in.h>
#elif defined(IPSO_ECHO)
#include <sys/types.h>
#include <netinet/in.h>
#else

#if defined (__cplusplus)
extern "C" {
#endif

struct in6_addr {
    unsigned char s6_addr[16];      /* IPv6 address */
};
#if defined (__cplusplus)
}
#endif
#endif


#if defined (__cplusplus)
extern "C" {
#endif

typedef struct in6_addr opsec_in6_addr;

/*
 * 128 Bits Unique Id Type
 */
typedef struct _opsec_uuid opsec_uuid;


/*
 * Boolean type definition
 */
typedef int opsec_boolean;
enum { OpsecFalse = 0, OpsecTrue = 1 }; 

/*------------------------------------------------------
 * OPSEC-wide virtual types
 *
 * New values should be added in the END of the typedef,
 * and their order should be KEPT!
 *------------------------------------------------------
 */

typedef enum {
    OPSEC_VT_ERROR=-1,
	OPSEC_VT_NONE=0,
	OPSEC_VT_INT,		/* 32 bits */
	OPSEC_VT_IP,		/* 32 bits network order */ 
	OPSEC_VT_PORT,		/* 16 bits unsigned */
	OPSEC_VT_PROTO,
	OPSEC_VT_LOG,
	OPSEC_VT_ACTION,
	OPSEC_VT_INTERFACE,
	OPSEC_VT_ALERT,
	OPSEC_VT_RULE,
	OPSEC_VT_DIRECTION,
	OPSEC_VT_IP_PROTO,
	OPSEC_VT_TCP_PORT,
	OPSEC_VT_UDP_PORT,
	OPSEC_VT_RPC_PROG,
	OPSEC_VT_HEX,
	OPSEC_VT_TIME,
	OPSEC_VT_STRING,
	OPSEC_VT_MASK,
	OPSEC_VT_DURATION_TIME,
	OPSEC_VT_SHORT,
	OPSEC_VT_USHORT,
	OPSEC_VT_BUFF,
	OPSEC_VT_UINT,
	OPSEC_VT_UI8BIT, /* integers */
	OPSEC_VT_I8BIT,
	OPSEC_VT_UI16BIT,
	OPSEC_VT_I16BIT,
	OPSEC_VT_UI32BIT,
	OPSEC_VT_I32BIT,
	OPSEC_VT_UI64BIT, /* 64 bits types */
	OPSEC_VT_I64BIT,
	OPSEC_VT_SR_HOSTNAME, /* server-resolved values */
	OPSEC_VT_SR_HOSTGROUP,
	OPSEC_VT_SR_USERGROUP,
	OPSEC_VT_SR_SERVICE,
	OPSEC_VT_SR_SERVICEGROUP,
	OPSEC_VT_ISTRING, /* case-insensitive string */
	OPSEC_VT_IPV6,
	OPSEC_VT_UUID,
	OPSEC_VT_LAST /* This must be the last value. New values should be added before it.
					 Should never be passed over the network. */	
} OPSEC_VT;

/*
 * Abstract type definitions
 */
typedef struct _opsec_value_t opsec_value_t;
typedef OPSEC_VT opsec_vtype;

/*
 * Function prototypes
 */
DLLIMP opsec_value_t *
opsec_value_create();

DLLIMP void
opsec_value_dest(opsec_value_t *val);

DLLIMP opsec_value_t *
opsec_value_dup(const opsec_value_t *val);

DLLIMP 
int opsec_value_set(opsec_value_t *val, OPSEC_VT type, ...);

DLLIMP 
int opsec_value_get(const opsec_value_t *val, ...);

DLLIMP int
opsec_value_copy(opsec_value_t *dest, const opsec_value_t *src);

DLLIMP OPSEC_VT
opsec_value_get_type(const opsec_value_t *val);


#if defined (__cplusplus)
} /*extern "C"*/
#endif /* __cplusplus */

#endif /* !__OPSEC_VT_API_H */

