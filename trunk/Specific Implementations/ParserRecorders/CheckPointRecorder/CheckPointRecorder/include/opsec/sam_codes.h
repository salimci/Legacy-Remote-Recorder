#ifndef _SAM_CODES_H_
#define _SAM_CODES_H_

#ifdef	__cplusplus
extern "C" {
#endif

/*
 * Codes for the different types of ACKs returned by SAM
 */

#define SAM_BAD_DG_ERR	-1
#define SAM_RESOLVE_ERR	-2

#define SAM_REQUEST_RECEIVED	1
#define SAM_MODULE_DONE			2
#define SAM_MODULE_FAILED		3
#define SAM_REQUEST_DONE		4
#define SAM_ALL_REQUESTS_DONE		5
#define SAM_END_SESSION_REQUEST_NOT_DONE	6
#define SAM_UNEXPECTED_END_OF_SESSION	6
#define SAM_MODULE_INVALID_REQUEST		7

#ifdef __cplusplus
}
#endif

#endif /* _SAM_CODES_H_ */
