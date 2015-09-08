#ifndef _AV_OVER_CVP_H_
#define _AV_OVER_CVP_H_

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * This include file defines the attributes and values of some
 * parameters which are passed between the client and server
 * in general cvp.
 *
 */


/*
 * The info in the client's request should contain the following:
 * It is the response of the SERVER to act according to the client
 * request.

content_name       http:url, ftp:filename, smtp:?
content_type       text/binary/compound/other
protocol           http=80/ftp=21/smtp=25/...
av_action          none/cure/check
return_file_mode:

 1) The server filters the content, and sends its reply (at some timepoint).
    The client will pass the data it gets from the server, and if the reply
	indicates that the content is "bad", the client should "kill" the connection.

 2) The same as 1 but the server sends the client data that cannot harm.
    (This is an odd mixture of 1 and 3).

 3) The servers sends its reply and then pass the filtered data. (current mode+)
    (passed tha data even if it did not  succeed to cure it)


additional information in info
which can be accessed by opsec_info_get(info,"source","ip",NULL) etc.

 source ip          (dotted format)
 source port
 ip_protocol        (e.g. tcp, udp, etc...)
 destination ip     
 destination port   
 user_name
*/
	
/*
 * posible values for action:
 */


#define	CVP_RDONLY 1	/* content cannot be modified or replaced */
#define	CVP_RDWR 2	/* read/write/replace */
#define	CVP_NONE 3	/* replacement allowed, no access to content */

/*
 * possible values for ret_mode:
 */
#define CVP_REPLY_ANYTIME 1	/* reply welcomed at any time */
#define CVP_SEND_SAFE_DATA 2	/* send only "safe" data before reply */
#define CVP_REPLY_FIRST 3	/* send data only after sending reply */
							  
/* possible values for protocol */
#define CVP_HTTP_PROTOCOL	80
#define CVP_SMTP_PROTOCOL	25
#define CVP_FTP_PROTOCOL	21
#define CVP_UNKNOWN_PROTOCOL	-1
/* any other value will indicate other protocol */

/* possible values for content type */
#define CVP_UNSPECIFIED_CONTENT	0
#define CVP_BIN_CONTENT	1
#define CVP_TEXT_CONTENT	2
#define CVP_COMPOUND_CONTENT	4

/*
 * prototype for seting/geting the request parameters from
 * the info-structure in cvp-request.
 */
DLLIMP int av_cvp_get_request_params(OpsecInfo *info, char **name, int *type,
	int *protocol, char **protocol_command, int *action, int *ret_mode);



/*
 * The server's reply contains opinion,log and info.
 * The opinion contains the following information:
 * 1) is the output content safe/unsafe.
 * 2) the content was modified/replaced
 * opinion can have one of the following values
 */
#define CVP_CANNOT_HANDLE_REQUEST	1
#define CVP_CONTENT_SAFE	2
#define CVP_CONTENT_UNSAFE	3
#define CVP_IS_CONTENT_SAFE(o)	((o&3)==CVP_CONTENT_SAFE)

/* CVP Masks */
#define IS_CONTENT_MODIFIED(n) (n & 12)
#define IS_ORIGINAL_CONTENT_SAFE(n) (n & 48)

/*
 * If it is not CANNOT_HANDLE_REQUEST, it should be ored with one of the follow
 */
#define CVP_CONTENT_MODIFIED	(3<<2)
#define CVP_CONTENT_REPLACED	(2<<2)
#define CVP_CONTENT_NOT_MODIFIED	(1<<2)
#define CVP_IS_CONTENT_CHANGED(o)	(!((o&(3<<2))==CVP_CONTENT_NOT_MODIFIED))
#define CVP_ORIGINAL_CONTENT_SAFETINESS_NOT_REPORTED (0<<4)   /* for compatability with servers that do not know this attr. */
#define CVP_ORIGINAL_CONTENT_SAFE (1<<4)  
#define CVP_ORIGINAL_CONTENT_UNSAFE (2<<4)
#define CVP_IS_ORIGINAL_CONTENT_SAFE(o)	((o&48)==CVP_ORIGINAL_CONTENT_SAFE)


/*
 * log should contain a short description of the server's action  which will
 * be entered to the FW1's log file.
 */

/*
 * Info may contain the following attributes
 *
 * warning    {NULL}
 *		The server can put here string that will reach the user via
 *		e.g. the browser. It can be used as a simple substitute
 *		for CONTENT_REPLACED .
 *
 */

#ifdef __cplusplus
}
#endif

#endif /* _AV_OVER_CVP_H_ */
