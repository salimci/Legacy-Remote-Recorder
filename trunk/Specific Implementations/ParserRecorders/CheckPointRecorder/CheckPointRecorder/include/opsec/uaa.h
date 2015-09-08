/*
 * OPSEC include file for 3rd party use.
 */
 
#ifndef _UAA_H
#define _UAA_H

#include "opsec_export_api.h"
#include <opsec/uaa_error.h>

#ifdef	__cplusplus
extern "C" {
#endif

/*
   Uaa server last/not-last reply enumeration
 */
 typedef enum {
 	UAA_REPLY_NOT_LAST      =  0,
 	UAA_REPLY_LAST          =  1
} UaaReplyIsLast;

/*
 * PreDefinition for used structures and types
 */ 
typedef struct _uaa_assert_t       uaa_assert_t;
typedef struct _uaa_assert_t_iter  uaa_assert_t_iter;


#define UAA_MAX_ASSERTIONS 50


/*
 * UAA generic API
 */

/* -------------------------------------------------------------------------
  |  uaa_assert_t_create:
  |  --------------------
  |  
  |  Description:
  |  ------------
  |  Creates a general UAA assert_t structure.
  |
  |  Parameters:
  |  -----------
  |  None.
  |
  |  Returned value:
  |  ---------------
  |  A pointer to uaa_assert_t if successful, NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP uaa_assert_t *uaa_assert_t_create();

/* -------------------------------------------------------------------------
  |  uaa_assert_t_destroy:
  |  ---------------------
  | 
  |  Description:
  |  ------------
  |  Destroys a UAA assert_t structure.
  |
  |  Parameters:
  |  -----------
  |  asserts - Returned by uaa_assert_t_create();
  |
  |  Returned value:
  |  ---------------
  |  None.
   ------------------------------------------------------------------------*/
  DLLIMP void uaa_assert_t_destroy(uaa_assert_t *asserts);

/* -------------------------------------------------------------------------
  |  uaa_assert_t_add:
  |  -----------------
  |
  |  Description:
  |  ------------
  |  Adds a new assertion to the a uaa_assert_t structure.
  |
  |  Parameters:
  |  -----------
  |  asserts - Returned by uaa_assert_t_create()
  |  type    - Type of the assertion.
  |  value   - Value of the assertion.
  |
  |  Returned value:
  |  ---------------
  |  Zero if successful. -1 otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP int uaa_assert_t_add(uaa_assert_t *asserts, const char *type, const char *value);

/* -------------------------------------------------------------------------
  |  uaa_assert_t_duplicate:
  |  -----------------------
  |  
  |  Description:
  |  ------------
  |  Duplicates a general UAA assert_t structure.
  |
  |  Parameters:
  |  -----------
  |  asserts - UAA assert_t structure to be duplicated.
  |
  |  Returned value:
  |  ---------------
  |  A pointer to uaa_assert_t if successful, NULL otherwise.
   ------------------------------------------------------------------------*/
  DLLIMP uaa_assert_t *uaa_assert_t_duplicate(uaa_assert_t *asserts);
  

/*
 *  UAA assert_t Iterator
 */

/* -------------------------------------------------------------------------
  |  uaa_assert_t_iter_create:
  |  -------------------------
  |  
  |  Description:
  |  ------------
  |  Creates a UAA asset_t iterator.
  |
  |  Parameters:
  |  -----------
  |  asserts - Returned from a call to uaa_assert_t_create() or an event handler.
  |  type    - Specifies the type of the assertion to be iterated on
  |            (NULL means full iteration).
  |
  |  Returned value:
  |  ---------------
  |  Pointer to a uaa_assert_t_iter object if succesfull, NULL otherwise.
  |
   -------------------------------------------------------------------------*/
  DLLIMP uaa_assert_t_iter *uaa_assert_t_iter_create(uaa_assert_t *asserts, const char *type);

/* -------------------------------------------------------------------------
  |  uaa_assert_t_iter_destroy:
  |  --------------------------
  |  
  |  Description:
  |  ------------
  |  Destroys a UAA assert_t iterator.
  |
  |  Parameters:
  |  -----------
  |  iter - Pointer to a UAA assert_t iterator.
  |
  |  Returned value:
  |  ---------------
  |  None.
   -------------------------------------------------------------------------*/
  DLLIMP void uaa_assert_t_iter_destroy(uaa_assert_t_iter *iter);

/* -------------------------------------------------------------------------
  |  uaa_assert_t_iter_get_next:
  |  ---------------------------
  |  
  |  Description:
  |  ------------
  |  Returns the next assertion in the 'uaa_assert_t' container.
  |
  |  Parameters:
  |  -----------
  |  iter - Pointer to a uaa_assert_t_iter object.
  |  name - On Output: Address of pointer to the type of the assertion(on success)
  |  type - On Output: Address of pointer to the value of the assertion(on success)
  |
  |  Returned value:
  |  ---------------
  |  Zero if successful, -1 otherwise. 
   -------------------------------------------------------------------------*/
   DLLIMP int uaa_assert_t_iter_get_next(uaa_assert_t_iter *iter,
                                         char             **val,
                                         char             **type);

 /* -------------------------------------------------------------------------
  |  uaa_assert_t_iter_reset:
  |  ------------------------
  |
  |  Description:
  |  ------------
  |  Sets the iterator to the beginning of the assertions container.
  |
  |  Parameters:
  |  -----------
  |  iter - Pointer to a uaa_assert_t_iter object.
  |
  |  Returned value:
  |  ---------------
  |  Zero if successful, -1 otherwise.
   -------------------------------------------------------------------------*/
  DLLIMP int uaa_assert_t_iter_reset(uaa_assert_t_iter *iter);

 /* -------------------------------------------------------------------------
  |  uaa_assert_t_compare:
  |  ------------------------
  |
  |  Description:
  |  ------------
  |  Compares two assert objects, except for a specified list of types.
  |
  |  Parameters:
  |  -----------
  |  a - Pointer to a uaa_assert_t object.
  |  b - Pointer to a uaa_assert_t object.
  |  ignore_list - Pointer to NULL terminated array of strings.
  |
  |  Returned value:
  |  ---------------
  |  Zero if equal, -1 otherwise.
   -------------------------------------------------------------------------*/
  DLLIMP int uaa_assert_t_compare(uaa_assert_t *a, uaa_assert_t *b, const char **ignore_list);

 /* -------------------------------------------------------------------------
  |  uaa_assert_t_n_elements:
  |  ------------------------
  |
  |  Description:
  |  ------------
  |  Returns the number of assertions in a uaa_assert_t structure
  |
  |  Parameters:
  |  -----------
  |  asserts - Pointer to a uaa_assert_t object
  |
  |  Returned value:
  |  ---------------
  |  Number of assertions if successful, -1 otherwise.
   -------------------------------------------------------------------------*/
  DLLIMP int uaa_assert_t_n_elements(uaa_assert_t *asserts);

/*
 *  Debug API
 */
 /* -------------------------------------------------------------------------
  |  uaa_assert_t_print:
  |  -------------------
  |
  |  Description:
  |  ------------
  |  Prints the data stored in a UAA assert_t structure.
  |
  |  Parameters:
  |  -----------
  |  asserts - Returned from a call to uaa_assert_t_create().
  |
  |  Returned value:
  |  ---------------
  |  None.
   -------------------------------------------------------------------------*/
 DLLIMP void uaa_print_assert_t(uaa_assert_t *asserts);


#ifdef __cplusplus
}
#endif

#endif /* _UAA_H */

