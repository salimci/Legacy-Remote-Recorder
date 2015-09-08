#ifndef _OPSEC_EXPORT_API_H
#define _OPSEC_EXPORT_API_H

#ifndef DLLIMP
#ifdef OPSEC_DLL_IMPORT
#define OPSEC_DLL
#endif
#  if (defined(WIN32) && defined(OPSEC_DLL))
#             define DLLIMP __declspec( dllimport )
#  else
#             define DLLIMP
#  endif
#endif


#endif
