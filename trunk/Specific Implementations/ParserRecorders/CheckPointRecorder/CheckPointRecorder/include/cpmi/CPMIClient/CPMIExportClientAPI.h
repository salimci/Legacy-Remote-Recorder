/* ///////////////////////////////////////////////////////////////////////////
CPMIExportClientAPI.h

                     
			   THIS FILE IS PART OF Check Point OPSEC SDK


/////////////////////////////////////////////////////////////////////////// */

#ifndef CPMIEXPORTCLIENTAPI_H_999241939B6511d3B7710090272CCB30
#define CPMIEXPORTCLIENTAPI_H_999241939B6511d3B7710090272CCB30


/*
 * On Windows:
 * Provide the __declspec specifier (dllimport for an .EXE file,
 * dllexport for .DLL).
 */
#ifndef CPMI_CLIENT_DLL_EXPORT
#	if defined (WIN32) && defined (CPMI_DLL)
#		define CPMI_CLIENT_DLL_EXPORT __declspec (dllimport)
#	else
#		define CPMI_CLIENT_DLL_EXPORT 
#	endif
#endif	/* CPMI_CLIENT_DLL_EXPORT */



/*
 * define the extern "C" when needed
 */
#ifdef __cplusplus
#   define CPMI_EXTERN_C extern "C"
#else
#   define CPMI_EXTERN_C 
#endif	/* __cplusplus */



/*
 * CPMI API declaration
 */
#ifndef CPMI_C_API
#   define CPMI_C_API CPMI_EXTERN_C CPMI_CLIENT_DLL_EXPORT
#endif /* CPMI_C_API */




#endif /*CPMIEXPORTCLIENTAPI_H_999241939B6511d3B7710090272CCB30*/
/* ///////////////////// EOF CPMIExportClientAPI.h ///////////////////// */

