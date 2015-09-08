/* ///////////////////////////////////////////////////////////////////////////
ExportSrvISApi.h
define CPSRVIS_EXPORT_API
/////////////////////////////////////////////////////////////////////////// */

#ifndef EXPORTSRVISAPI_H_77380161CCF711d3B7B50090272CCB29
#define EXPORTSRVISAPI_H_77380161CCF711d3B7B50090272CCB29

/*
 * On Windows:
 * Provide the storage class specifier (extern for an .exe file, null
 * for DLL) and the __declspec specifier (dllimport for an .exe file,
 * dllexport for DLL).
 */


/*
 * used to export classes/apis from dlls
 */
#if !defined (CPSRV_EXPORT_API)
#   if defined (WIN32) && defined (SRVIS_DLL)
#       define CPSRV_EXPORT_API __declspec (dllimport)
#   else /*WIN32 && SRVIS_DLL*/
#       define CPSRV_EXPORT_API   
#   endif /*WIN32 && SRVIS_DLL*/
#endif /*CPSRV_EXPORT_API*/


/*
 * define the extern "C" when needed
 */
#if defined (__cplusplus)
#   define CPSRVIS_EXTERN_C extern "C"
#else
#   define CPSRVIS_EXTERN_C 
#endif	/* __cplusplus */



/*
 * CPMI API declaration
 */
#ifndef CPSRV_EXPORT_C_API
#   define CPSRV_EXPORT_C_API CPSRVIS_EXTERN_C CPSRV_EXPORT_API
#endif /* CPMI_C_API */






#endif /*EXPORTSRVISAPI_H_77380161CCF711d3B7B50090272CCB29*/

/* //////////////////////// EOF ExportSrvISApi.h //////////////////////// */



