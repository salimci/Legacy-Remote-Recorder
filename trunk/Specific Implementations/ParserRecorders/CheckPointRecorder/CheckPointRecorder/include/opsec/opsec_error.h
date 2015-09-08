#ifndef _OPSEC_ERROR_H_
#define _OPSEC_ERROR_H_

#include "opsec_export_api.h"

#ifdef __cplusplus
extern "C" {
#endif

DLLIMP extern int opsec_errno;
DLLIMP extern char *opsec_errno_str(int err);


/*
 * List of possible opsec_errno
 */
#define EO_ERROR         -1  /* general Error                                   */ 
#define EO_OK             0  /* NO Error                                        */
#define EO_UNKNOWN        1  /* Unknown Error                                   */
#define EO_SYS_SOCK       2  /* Socket layer error                              */
#define EO_SYS            3  /* System error (look at errno)                    */
#define EO_NULLARG        4  /* Argument is NULL or lacks some data             */
#define EO_COMMTYPE       5  /* Unknown comm type (Should not happen) !         */
#define EO_SRVNOTRUN      6  /* Server is not running (localy)                  */
#define EO_BADSESID       7  /* Bad session ID value                            */
#define EO_COMMNOTCON     8  /* Comm is not connected/ no peer in internal comm */
#define EO_BADDG          9  /* Recieved datagram is bad                        */
#define EO_BADATTR       10  /* BAD attribute in vararg routine                 */
#define EO_SESCLOSED     11  /* Session is closed (just before destroy)         */
#define EO_MALLOC        12  /* Failed to malloc                                */
#define EO_OPENF         13  /* Failed to open file                             */
#define EO_CSOCKET       14  /* Error during socket creation                    */
#define EO_DGLEN         15  /* Datagram length is too short for reading        */
#define EO_DGNOMEM       16  /* Datagram length is too short for writing        */
#define EO_BADINFO       17  /* BAD type of opsec-info  or other info error     */
#define EO_COMMCONGESTED 18  /* Communication path is congested                 */
#define EO_SIC_INIT      19  /* SIC intialization error                         */
#define EO_MULTI_SIC     20  /* Multiple-SIC identity error                     */


#ifdef __cplusplus
}
#endif

#endif /* _OPSEC_ERROR_H_ */
