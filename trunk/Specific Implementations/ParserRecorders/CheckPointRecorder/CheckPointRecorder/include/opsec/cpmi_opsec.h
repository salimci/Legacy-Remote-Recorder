/************************************************************************
*                                                                       *
*   cpmi_opsec.h --                                                     *
*       This module defines the common API's for the client and server  *
*                                                                       *
************************************************************************/

#ifndef _CPMI_OPSEC_H
#define _CPMI_OPSEC_H

#include "opsec/opsec.h"


#ifdef __cplusplus
extern "C" {
#endif
	

/*
 * Client Datagram types
 */ 

#define 	OPSEC_CPMI_CANCEL_REQUEST 	    _NEW_DGRAM_TYPE(OPSEC_CPMI_CLIENT, 1)  
#define 	OPSEC_CPMI_GENERIC_REQUEST      _NEW_DGRAM_TYPE(OPSEC_CPMI_CLIENT, 2)
#define 	OPSEC_CPMI_NO_DUP_GENERIC_REQUEST       _NEW_DGRAM_TYPE(OPSEC_CPMI_CLIENT, 3)

/*
 * Server Datagram types
 */ 
#define     OPSEC_CPMI_SERVER_REPLY          _NEW_DGRAM_TYPE(OPSEC_CPMI_SERVER, 1)  
#define     OPSEC_CPMI_NO_DUP_SERVER_REPLY          _NEW_DGRAM_TYPE(OPSEC_CPMI_SERVER, 2)


/*
 * operation ID
 */
typedef unsigned int CPMIOpId;

/*
 * CPMI Attributes (bitmask)
 */
#define CPMI_ATT_LAST_CHUNK      (1)
#define CPMI_ATT_CHUNKED         (1<<1)
#define CPMI_ATT_CONT_CHUNK      (1<<2)

#define CPMI_SET_BIT(bm,bt)      ((bm) |= bt)
#define CPMI_IS_BIT(bm,bt)       ((bm) & bt)
#define CPMI_CLEAR_BIT(bm,bt)    ((bm) &= ~bt)

#define CPMI_SET_LAST_REPLY(bm)   CPMI_SET_BIT(bm,CPMI_ATT_LAST_CHUNK)
#define CPMI_IS_LAST_REPLY(bm)    CPMI_IS_BIT(bm,CPMI_ATT_LAST_CHUNK)
#define CPMI_CLEAR_LAST_REPLY(bm) CPMI_CLEAR_BIT(bm,CPMI_ATT_LAST_CHUNK)

#define CPMI_SET_LAST_CHUNK(bm)   CPMI_SET_BIT(bm,CPMI_ATT_LAST_CHUNK)
#define CPMI_IS_LAST_CHUNK(bm)    CPMI_IS_BIT(bm,CPMI_ATT_LAST_CHUNK)
#define CPMI_CLEAR_LAST_CHUNK(bm) CPMI_CLEAR_BIT(bm,CPMI_ATT_LAST_CHUNK)

#define CPMI_SET_CHUNKED(bm)     CPMI_SET_BIT(bm,CPMI_ATT_CHUNKED)
#define CPMI_IS_CHUNKED(bm)      CPMI_IS_BIT(bm,CPMI_ATT_CHUNKED)
#define CPMI_CLEAR_CHUNKED(bm)   CPMI_SET_BIT(bm,CPMI_ATT_CHUNKED)

#define CPMI_SET_CONT_CHUNK(bm)    CPMI_SET_BIT(bm,CPMI_ATT_CONT_CHUNK)
#define CPMI_IS_CONT_CHUNK(bm)    CPMI_IS_BIT(bm,CPMI_ATT_CONT_CHUNK)
#define CPMI_CLEAR_CONT_CHUNK(bm) CPMI_SET_BIT(bm,CPMI_ATT_CONT_CHUNK)

#define CPMI_DG_HEADER_LEN 16

/* Communication Signals */
typedef enum { 
	CpmiCommConnected = 0,
	CpmiCommOutEmpty = 1,
	CpmiCommErr = 2
} eCpmiCommSignal;
 
#ifdef __cplusplus
}
#endif

#endif /* _CPMI_OPSEC_H */
