#include "opsec/opsec_vt_api.h"
#include "opsec/opsec_error.h"

/*
 * OPSEC UUID APIs
 */

#ifndef __OPSEC_UUID_H
#define __OPSEC_UUID_H

#ifdef __cplusplus
extern "C" {
#endif

DLLIMP opsec_uuid *opsec_uuid_create();
DLLIMP void opsec_uuid_destroy(opsec_uuid *uuid);
DLLIMP opsec_uuid *opsec_uuid_duplicate(opsec_uuid *uuid);
DLLIMP int opsec_uuid_set_unspecified(opsec_uuid *uuid);
DLLIMP int opsec_uuid_equal(opsec_uuid *uuid1, opsec_uuid *uuid2);
DLLIMP int opsec_uuid_to_string(opsec_uuid *uuid, char *str_buf);
DLLIMP int opsec_uuid_from_string(opsec_uuid *uuid, const char *str);

#ifdef __cplusplus
}
#endif

#endif /* !__OPSEC_UUID_H */
