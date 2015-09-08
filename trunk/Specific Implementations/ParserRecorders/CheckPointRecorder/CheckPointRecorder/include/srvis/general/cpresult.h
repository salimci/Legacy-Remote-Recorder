/*  //////////////////////////////////////////////////////////////////////////
cpresult.h
definitions of functions, macros and types to be used as
general error handling mechanism in Check Point
//////////////////////////////////////////////////////////////////////////  */
#ifndef CPRESULT_H_C44D6162089C11D4B7EA0090272CCB30
#define CPRESULT_H_C44D6162089C11D4B7EA0090272CCB30

#include "srvis/general/ExportSrvISApi.h"


/*
cpresult is a 32-bit value with several fields encoded in the value.  
The parts of cpresult are shown below.

  3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
  1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
 +-+-+-+-+-+---------------------+-------------------------------+
 |S|         Facility            |               Code            |
 +-+-+-+-+-+---------------------+-------------------------------+

 where
     S - Severity - indicates success/fail
         0 - Success
         1 - Fail
     Facility - is the facility code
     Code - is the facility's status code

New values should use CP_FACILITY_CP as the facility and error codes
in the range 0x0200 - 0xFFFF.

We strive to assign each error code an error message. To get the message
describin an error, call CPGetErrorMessage().
*/


#if !defined (CPRESULT_DEFINED)
#define CPRESULT_DEFINED
/** cpresult declaration */
typedef int cpresult;
#endif /*CPRESULT_DEFINED*/



/** 
Get a description for a given cpresult. 
@param  nErr [IN] Error code
@return The message associated with given error. 
        NULL is returned when the error message is not found  */
CPSRV_EXPORT_C_API const char* 
CPGetErrorMessage (cpresult nErr);



/* Severity values */
enum {
	CP_SEVERITY_SUCCESS = 0,
	CP_SEVERITY_ERROR   = 1
};



/* Facility codes */
enum {
	CP_FACILITY_NULL = 0,  /* General errors. Corresponds to COM's FACILITY_NULL     */
	CP_FACILITY_CP   = 4,  /* General Check Point. Corresponds to COM's FACILITY_ITF */
	CP_FACILITY_OS   = 7,  /* From OS. Corresponds to COM's FACILITY_WIN32           */
	CP_FACILITY_NET  = 12  /* Network errors. Corresponds to COM's FACILITY_INTERNET */
};



/* Generic test for error on any status value */
#define CPRESULT_IS_ERROR(_code)    \
    (((unsigned int)(_code)) >> 31 == CP_SEVERITY_ERROR)



/* Return the code */
#define CPRESULT_CODE(_code)               ((_code) & 0xFFFF)



/* Return the facility */
#define CPRESULT_FACILITY(_code)           (((_code) >> 16) & 0x1FFF)



/* Return the severity */
#define CPRESULT_SEVERITY(_code)           (((_code) >> 31) & 0x1)



/* Generic test for success on any status value */
#define CP_SUCCEEDED(_code)                (((cpresult)(_code)) >= 0)



/* And the inverse */
#define CP_FAILED(_code)                   (((cpresult)(_code)) < 0)



/* Create a cpresult value from number */
#define CP_DECLARE_CPRESULT(_code)         ((cpresult)(_code))



/* Create a cpresult value from name and number */
#define CP_DECLARE_CPRESULT2(_name,_code,_msg)  \
    enum { _name = ((cpresult)(_code)) }



/* Create a cpresult from an OS error code */
#define CPRESULT_FROM_OS_CODE(_code)                  \
    ((cpresult) ( ( (_code) & 0x0000FFFF)  |          \
                    (CP_FACILITY_OS << 16) |          \
                    0x80000000 ) )



/* Create a cpresult from an OS error code */
#define CP_DECLARE_CPRESULT_FROM_OS_CODE(_name,_code,_msg)  \
    enum { _name = CPRESULT_FROM_OS_CODE(_code) }



/* Create a cpresult value from component pieces */
#define CP_MAKE_CPRESULT(_sev,_fac,_code)             \
    ((cpresult) ( ( ((unsigned int)(_sev)) << 31) |   \
                  ( ((unsigned int)(_fac)) << 16) |   \
                  ( ((unsigned int)(_code)) ) ) )





#endif /* CPRESULT_H_C44D6162089C11D4B7EA0090272CCB30 */
/* ///////////////  EOF cpresult.h  /////////////// */



