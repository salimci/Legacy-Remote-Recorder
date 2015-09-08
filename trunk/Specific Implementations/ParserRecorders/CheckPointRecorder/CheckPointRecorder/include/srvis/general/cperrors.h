/* /////////////////////////////////////////////////////////////////////////////
cperrors.h
Declaration of general-purpose cpresult codes
///////////////////////////////////////////////////////////////////////////// */
#ifndef CPERR_H_451DFBAE9D0C44C9B6EF00AD1E48146D
#define CPERR_H_451DFBAE9D0C44C9B6EF00AD1E48146D

#include "srvis/general/cpresult.h"




/* general success
 * Note: value match COM's S_OK */
CP_DECLARE_CPRESULT2 (CP_S_OK, 0x00000000L, "The Operation Finished Successfully");



/* general success
 * Note: value match COM's S_FALSE */
CP_DECLARE_CPRESULT2 (CP_S_FALSE, 0x00000001L, "The Operation Finished Successfully");



/* Unspecified error
 * Note: value match COM's E_FAIL 
 * -2147467259 */
CP_DECLARE_CPRESULT2 (CP_E_FAIL ,0x80004005L, "Unspecified error");



/* Requested Operation is not implemented
 * Note: value match COM's E_NOTIMPL 
 * -2147467263 */
CP_DECLARE_CPRESULT2 (CP_E_NOTIMPL ,0x80004001L, "Requested Operation is not implemented");



/* There is no more memory for normal functioning
 * Note: value match COM's E_OUTOFMEMORY 
 * -2147024882 */
CP_DECLARE_CPRESULT2 (CP_E_OUTOFMEMORY ,0x8007000EL, "Ran out of memory");



/* One or more arguments are invalid
 * Note: value match COM's E_INVALIDARGS 
 * -2147024809 */
CP_DECLARE_CPRESULT2 (CP_E_INVALIDARG ,0x80070057L, "One or more arguments are invalid");



/* Invalid pointer
 * Note: value match COM's E_POINTER 
 * -2147467261 */
CP_DECLARE_CPRESULT2 (CP_E_POINTER ,0x80004003L, "Invalid pointer");



/* Invalid handle
 * Note: value match COM's E_HANDLE 
 * -2147024890 */
CP_DECLARE_CPRESULT2 (CP_E_HANDLE ,0x80070006L, "Invalid handle");


/* operation aborted
 * Note: value match COM's E_ABORT 
 * -2147467260 */
CP_DECLARE_CPRESULT2 (CP_E_ABORT ,0x80004004L, "Operation aborted"); 



/* General access denied error
 * Note: value match COM's E_ACCESSDENIED 
 * -2147024891 */
CP_DECLARE_CPRESULT2 (CP_E_ACCESSDENIED ,0x80070005L, "General access denied error");



/* Catastrophic failure
 * Note: value match COM's E_UNEXPECTED 
 * -2147418113 */
CP_DECLARE_CPRESULT2 (CP_E_UNEXPECTED ,0x8000FFFFL, "Catastrophic failure");



/* Can't iterate any more.
 * Note: value match COM's OLE_E_ENUM_NOMORE
 * -2147221502 */
CP_DECLARE_CPRESULT2 (CP_E_ITER_NOMORE ,0x80040002L, "Can't iterate any more, because the associated data is missing");



/* network time out
 * Note: value match COM's RPC_E_TIMEOUT 
 * -2146696929 */
CP_DECLARE_CPRESULT2 (CP_E_TIMEOUT ,0x800C011FL, "This operation returned because the timeout period expired");



/* Operation is pending
 * Note: value match COM's E_PENDING 
 * -2147483638 */
CP_DECLARE_CPRESULT2 (CP_E_PENDING, 0x8000000AL, "The data necessary to complete this operation is not yet available");



/* No such interface supported
 * Note: value match COM's E_NOINTERFACE
 * -2147467262 */
CP_DECLARE_CPRESULT2 (CP_E_NOINTERFACE, 0x80004002L, "No such interface supported");



/* In-process DLL or handler DLL not found
 * Note: value match COM's CO_E_DLLNOTFOUND 
 * -2147221000 */
CP_DECLARE_CPRESULT2 (CP_E_DLLNOTFOUND, 0x800401F8L, "DLL not found");



/* Class not registered
 * Note: value match COM's REGDB_E_CLASSNOTREG
 * -2147221164 */
CP_DECLARE_CPRESULT2 (CP_E_CLASSNOTREG, 0x80040154L, "Class not registered");



/* Class does not support aggregation (or class object is remote)
 * Note: value match COM's CLASS_E_NOAGGREGATION
 * -2147221232 */
CP_DECLARE_CPRESULT2 (CP_CLASS_E_NOAGGREGATION, 0x80040110L, "Class does not support aggregation (or class object is remote)");


/* An unexpected network error occurred.
 * Note: value match WIN32's ERROR_UNEXP_NET_ERR 
 * 0x8007003B, -2147024837 */
CP_DECLARE_CPRESULT_FROM_OS_CODE (CP_E_UNEXP_NET_ERR, 59L, "An unexpected network error occurred");


/* A disk error occurred during a write operation.
 * Note: value match COM's STG_E_WRITEFAULT 
 * -2147287011 */
CP_DECLARE_CPRESULT2 (CP_E_WRITEFAULT, 0x8003001DL, "A disk error occurred during a write operation");


/* A disk error occurred during a read operation. 
 * Note: value match COM's STG_E_READFAULT
 * -2147287010 */
CP_DECLARE_CPRESULT2 (CP_E_READFAULT, 0x8003001EL, "A disk error occurred during a read operation");


/* There is insufficient disk space to complete operation.
 * Note: value match COM's STG_E_MEDIUMFULL 
 * -2147286928 */
CP_DECLARE_CPRESULT2 (CP_E_MEDIUMFULL, 0x80030070L, "There is insufficient disk space to complete operation");


/* This implementation's limit for advisory connections has been reached.
 * Note: value match COM's CONNECT_E_ADVISELIMIT
 * -2147220991 */
CP_DECLARE_CPRESULT2 (CP_CONNECT_E_ADVISELIMIT, 0x80040201, "This implementation's limit for advisory connections has been reached");


/* Connection attempt failed.
 * Note: value match COM's CONNECT_E_CANNOTCONNECT
 * -2147220990 */
CP_DECLARE_CPRESULT2 (CP_CONNECT_E_CANNOTCONNECT, 0x80040202L, "Connection attempt failed");


/* There is no connection for this connection id.
 * Note: value match COM's CONNECT_E_NOCONNECTION
 * -2147220992 */
CP_DECLARE_CPRESULT2 (CP_CONNECT_E_NOCONNECTION, 0x80040200L, "There is no connection for this connection id");


/* Must use a derived interface to connect
 * Note: value match COM's CONNECT_E_OVERRIDDEN
 * -2147220989 */
CP_DECLARE_CPRESULT2 (CP_CONNECT_E_OVERRIDDEN, 0x80040203L, "Must use a derived interface to connect");


/* Invalid thread identifier
 * 0x800705A4, -2147023452 */
CP_DECLARE_CPRESULT_FROM_OS_CODE (CP_E_INVALID_THREAD_ID, 1444L, "Invalid thread identifier");


/* Cannot create another thread
 * 0x8007009B, -2147024741 */
CP_DECLARE_CPRESULT_FROM_OS_CODE (CP_E_THREAD_CREATE, 155L, "Cannot create another thread");


/* Object is already registered.
 * note: value match COM's CO_E_OBJISREG
 * */
CP_DECLARE_CPRESULT2 (CP_E_OBJISREG, 0x800401FCL, "Object is already registered");


/* Object is not registered
 * Note: value match COM's CO_E_OBJNOTREG
 *  */
CP_DECLARE_CPRESULT2 (CP_E_OBJNOTREG, 0x800401FBL, "Object is not registered");


/* ClassFactory cannot supply requested class
 * Note: value match COM's CLASS_E_CLASSNOTAVAILABLE
 *  */
CP_DECLARE_CPRESULT2 (CP_CLASS_E_CLASSNOTAVAILABLE, 0x80040111L, "ClassFactory cannot supply requested class");






#endif /* CPERR_H_451DFBAE9D0C44C9B6EF00AD1E48146D */
/* //////////////////////////// EOF cperrors.h //////////////////////////// */


