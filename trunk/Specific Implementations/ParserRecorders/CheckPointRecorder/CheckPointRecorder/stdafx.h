/*
 * CheckPoint Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

// TODO: reference additional headers your program requires here
#define NUMBER_AIDX_FIELDS	21

#define AIDX_NUM		0
#define AIDX_TIME		1
#define AIDX_ACTION		2
#define AIDX_ORIG		3
#define AIDX_IF_DIR		4
#define AIDX_IF_NAME		5
#define AIDX_HAS_ACCOUNTING	6
#define AIDX_UUID		7
#define AIDX_PRODUCT		8
#define AIDX_OBJECTNAME		9
#define AIDX_OBJECTTYPE		10
#define AIDX_OBJECTTABLE 	11
#define AIDX_OPERATION		12
#define AIDX_UID		13
#define AIDX_ADMINISTRATOR	14
#define AIDX_MACHINE		15
#define AIDX_SUBJECT		16
#define AIDX_AUDIT_STATUS	17
#define AIDX_ADDITIONAL_INFO	18
#define AIDX_OPERATION_NUMBER	19
#define AIDX_FIELDSCHANGES	20

#define INITIAL_CAPACITY   1024
#define CAPACITY_INCREMENT 4096

#ifdef SOLARIS2
#	define  BIG_ENDIAN    4321
#	define  LITTLE_ENDIAN 1234
#	define  BYTE_ORDER BIG_ENDIAN
#	define  SLEEP(sec) sleep(sec)
#	include <netinet/in.h>
#	include <arpa/inet.h>
#	include <syslog.h>
#	include <unistd.h>
#elif WIN32
#	define  BIG_ENDIAN    4321
#	define  LITTLE_ENDIAN 1234
#	define  BYTE_ORDER LITTLE_ENDIAN
#	define  BUFSIZE MAX_PATH
#	define  SLEEP(sec) Sleep(1000*sec)
#	include <windows.h>
#	include <winsock.h>
#else
#	define  SLEEP(sec) sleep(sec)
#	include <netinet/in.h>
#	include <arpa/inet.h>
#	include <unistd.h>
#	include <endian.h>
#	include <syslog.h>
#endif


