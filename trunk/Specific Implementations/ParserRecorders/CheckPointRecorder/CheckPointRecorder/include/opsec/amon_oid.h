/**********************************************************
 *
 * Oid API Definitions
 *
 **********************************************************/

#ifndef _OID_H_
#define _OID_H_

#ifndef DLLIMP
#  if (defined(WIN32) && defined(OPSEC_DLL_IMPORT))
#	      define DLLIMP __declspec( dllimport )
#  else
#	      define DLLIMP
#  endif
#endif

#ifdef __cplusplus
extern "C" {
#endif


/*
 * OidNum
 */
typedef unsigned int OidNum;

#define SIZEOF_OID_NUM(oid_num)     sizeof(unsigned int)


typedef enum { 
    OidContain_NoContainment,
    OidContain_LeftContainRight,
    OidContain_RightContainLeft,
    OidContain_Identical,
    OidContain_Error
} eOidContain;


/***********************************************************
 *
 * Oid API
 *
 ***********************************************************/
typedef struct _Oid     Oid;

DLLIMP int 
oid_create(Oid **oid,
           const OidNum *oid_arr,
           unsigned int oid_arr_len);

DLLIMP int 
oid_create_from_string(Oid **oid, 
                       const char *oid_str);

DLLIMP int
oid_duplicate(Oid **dst_oid, 
              const Oid *src_oid);

DLLIMP void 
oid_destroy(Oid *oid);

DLLIMP char * 
oid_to_string(const Oid *oid);

DLLIMP int
oid_to_array(const Oid *oid,
             OidNum **oid_arr,
             unsigned int *oid_arr_len);

DLLIMP unsigned int
oid_get_length(const Oid *oid);

DLLIMP int
oid_compare(const Oid *left, 
            const Oid *right);

DLLIMP int
oid_concat(Oid* oid1, 
           const Oid* oid2);

DLLIMP eOidContain 
oid_contain(const Oid* left, 
            const Oid* right);

DLLIMP int
oid_prefix(const Oid* oid, 
           unsigned int num_of_elems, 
           Oid** prefix_oid);           

DLLIMP int
oid_suffix(const Oid* oid, 
           unsigned int num_of_elems, 
           Oid** suffix_oid);

DLLIMP void
oid_chop_left(Oid* oid, 
              unsigned int num_of_elems);

DLLIMP void
oid_chop_right(Oid* oid, 
               unsigned int num_of_elems);

DLLIMP int
oid_element(const Oid *oid,
            unsigned int index);
            

#ifdef __cplusplus
}
#endif

#endif /* _OID_H_ */

