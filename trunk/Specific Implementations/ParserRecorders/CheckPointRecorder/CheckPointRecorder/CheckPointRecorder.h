/*
* CheckPoint Recorder
* Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
* You have no rights to distrubute, modify and use this code unless writer gives permission
*/

#pragma once
#include "opsec/lea.h"
#include "opsec/lea_filter.h"
#include "opsec/lea_filter_ext.h"
#include "opsec/opsec.h"
#include "opsec/opsec_error.h"
#include "Lea2RecField.h"

using namespace System;
#include<iostream>
using namespace std;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace Microsoft::Win32;
using namespace CustomTools;
using namespace Log;
using namespace System::Threading;
using namespace  System::Collections::Generic;
using namespace System::Globalization;
using namespace System::Data;
using namespace System::Data::SqlClient;

#undef ERROR

namespace Parser
{
#define CONFIG_LENGTH 22

	typedef void (*OpsecSdkFunction)(OpsecSession*,int,int,int,char*,char*);
	int run(int length, char **args);

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	delegate int read_fw1_logfile_record_delegate(OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[]);
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	delegate int opsec_callback_delegate(OpsecSession * pSession);
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	delegate void printRemoteSdkVersionDelegate(OpsecSession *pSession,int sdk_version,int patch_num,int build_num,char *ver_desc,char *full_desc);

	public ref class CheckPointRecorder : CustomBase
	{
	private:
		System::Object^ syncRoot;
		int status; // Currently 0: idle, 1: running

		array<IntPtr>^ memToFree;

		int Id;
		String^ VirtualHost;
		String^ Dal;
		RegistryKey^ reg;
		read_fw1_logfile_record_delegate^ read_fw1_logfile_record_del;
		opsec_callback_delegate^ read_fw1_logfile_eof_del;
		opsec_callback_delegate^ read_fw1_logfile_collogs_del;
		opsec_callback_delegate^ read_fw1_logfile_suspend_del;
		opsec_callback_delegate^ read_fw1_logfile_resume_del;
		opsec_callback_delegate^ read_fw1_logfile_session_start_del;
		opsec_callback_delegate^ read_fw1_logfile_session_end_del;
		opsec_callback_delegate^ read_fw1_logfile_established_del;
		printRemoteSdkVersionDelegate^ printVersionDel;

	public:
		Dictionary<String^, Lea2RecField^>^ LeaMappersLookup;
		Dictionary<String^,CheckPointRecorder^>^ subRecorders;
		Dictionary<String^,Object^>^ customData;
		String^ auth_type;
		String^ ip;
		String^ auth_port;
		String^ opsec_entity_sic_name;
		String^ opsec_sic_name;
		String^ opsec_sslca_file;


		bool fileCheckComplete;
		int goOnWithServerReply;

		String^ outputFile;
		String^ outputFile2;
		String^ subRecorderId;
		bool isMultiMode;

		int recoveryMode;
		Thread^ thread;
		bool needLastPositionReset;

		String^ parametricFilename;
		String^ filename;
		long fileId;
		bool eof;
		int lastPosition;
		int lastProcessed;
		String^ lastRecordDateTime;

		int totalPass;
		int totalProcessed;

		bool fw1_2000;
		bool online_mode;

		int opsec_debug_level;
		int maxLogFiles;
		int maxRecord;

		Log::CLogger^ log;		
		bool usingRegistry;

		OpsecEntity *pClient;
		OpsecEntity *pServer;
		OpsecSession *pSession;
		OpsecEnv *pEnv;
		String^ isolatedRegistry;
		long stopOnFileId;
		int useCollectedFiles;
		int justPrint;
		SqlConnection^ con;
		SqlCommand^ sqlCmd;
		String^ connectionString;

		CheckPointRecorder()
		{
			InitializeCheckPointRecorder(nullptr,"");
		}

		CheckPointRecorder(String^ isolatedRegistry,String^ subRecorderId)
		{
			InitializeCheckPointRecorder(isolatedRegistry,String::IsNullOrEmpty(subRecorderId) ? "" : subRecorderId);
		}


		char * ResolveField(OpsecSession * pSession,lea_field *field,char *buffer)
		{
			if (field->lea_val_type == LEA_VT_IP_ADDR) {
				unsigned long ul = field->lea_value.ul_value;
				if (BYTE_ORDER == LITTLE_ENDIAN)
				{
					sprintf (buffer, "%d.%d.%d.%d", (int) ((ul & 0xff) >> 0),
						(int) ((ul & 0xff00) >> 8),
						(int) ((ul & 0xff0000) >> 16),
						(int) ((ul & 0xff000000) >> 24));
				}
				else
				{
					sprintf (buffer, "%d.%d.%d.%d",
						(int) ((ul & 0xff000000) >> 24),
						(int) ((ul & 0xff0000) >> 16),
						(int) ((ul & 0xff00) >> 8),
						(int) ((ul & 0xff) >> 0));
				}
				return buffer;
			}
			if (field->lea_val_type == LEA_VT_TCP_PORT || field->lea_val_type == LEA_VT_UDP_PORT) {
				unsigned short us = field->lea_value.ush_value;
				if (BYTE_ORDER == LITTLE_ENDIAN)
				{
					us = (us >> 8) + ((us & 0xff) << 8);
				}
				sprintf (buffer, "%d", us);
				return buffer;
			}
			return lea_resolve_field(pSession,*field);
		}

		void InitializeCheckPointRecorder(String^ isolatedRegistry,String^ subRecorderId) {
			memToFree=nullptr;
			if (String::Equals(subRecorderId,""))
				subRecorders=gcnew Dictionary<String^,CheckPointRecorder^>();
			con=nullptr;
			sqlCmd=nullptr;
			this->isolatedRegistry=isolatedRegistry;
			customData=gcnew Dictionary<String^,Object^>();
			pClient=NULL;
			pServer=NULL;
			pSession=NULL;
			pEnv=NULL;
			this->subRecorderId=subRecorderId;
			syncRoot=gcnew System::Object();
			status=0;
			Id = 0;
			VirtualHost = "";
			Dal = "";
			reg=nullptr;

			needLastPositionReset=false;
			filename=nullptr;
			fileId=0;
			eof=false;
			lastPosition=0;
			totalPass=0;


			fw1_2000 = FALSE;
			online_mode = FALSE;

			usingRegistry = TRUE;

			maxLogFiles=0;
			lastPosition = 0;

			log = gcnew CLogger();
			LeaMappersLookup=gcnew Dictionary<String^, Lea2RecField^>();
			InitializeDictionary(LeaMappersLookup);
			Init();
		}

		~CheckPointRecorder()
		{
			if (reg != nullptr) {
				try {
					reg->Close();
				} catch(Exception^ ex) {}
				reg=nullptr;
			}
		}
#define ENABLE_BREAK_POINT 1
		int BreakPoint(String^ msg) 
		{
			Console::WriteLine(msg);
			if(ENABLE_BREAK_POINT) {
				Console::Write("Enter to continue:");
				Console::ReadLine();
			}
			return 1;
		}

		bool setLea2RecComputerName(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs,Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecComputerName");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->ComputerName=System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)leaField);
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecComputerName] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt1(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt1");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt1=leaField != NULL ? atoi(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt1] " + ex->Message +"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecCustomInt2(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt2");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt2=leaField != NULL ? atoi(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt2] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt3(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt3");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt3=leaField != NULL ? atoi(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt3] " + ex->Message +"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecCustomInt4(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt4");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt4=leaField != NULL ? atoi(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt4] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt5(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt5");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt5=leaField != NULL ? atoi(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt5] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt6(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt6");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt6=leaField != NULL ? atol(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt6] " + ex->Message +"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecCustomInt7(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt7");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt7=leaField != NULL ? atol(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt7] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt8(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt8");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt8=leaField != NULL ? atol(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt8] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt9(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt9");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt9=leaField != NULL ? atol(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt9] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomInt10(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomInt10");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomInt10=leaField != NULL ? atol(leaField) : 0;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomInt0] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr1(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr1");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomStr1=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr1] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr2(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr2");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomStr2=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr2] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr3(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr3");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomStr3=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr3] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr4(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr4");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->CustomStr4=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr4] " + ex->Message +"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecCustomStr5(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr5");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr5 != nullptr && localRecord->CustomStr5->Length > 0)
						localRecord->CustomStr5+=" ";
					localRecord->CustomStr5+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr5] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr6(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr6");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr6 != nullptr && localRecord->CustomStr6->Length > 0)
						localRecord->CustomStr6+=" ";
					localRecord->CustomStr6+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr6] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr7(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr7");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr7 != nullptr && localRecord->CustomStr7->Length > 0)
						localRecord->CustomStr7+=" ";
					localRecord->CustomStr7+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr7] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr8(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr8");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr8 != nullptr && localRecord->CustomStr8->Length > 0)
						localRecord->CustomStr8+=" ";
					localRecord->CustomStr8+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr8] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr9(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr9");
				}
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr9 != nullptr && localRecord->CustomStr9->Length > 0)
						localRecord->CustomStr9+=" ";
					localRecord->CustomStr9+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr9] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecCustomStr10(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				CheckPointRecorder^ cpr=CheckPointRecorder::getInstance();
				//Check if url already set
				if (cpr != nullptr) {
					Object^ v;
					if (customData->TryGetValue("urlOk",v)
						&& (bool)v) {
							return true;
					}
				}
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecCustomStr10");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				if (leaField != nullptr && leaField[0] != '\0') {
					if (localRecord->CustomStr10 != nullptr && localRecord->CustomStr10->Length > 0)
						localRecord->CustomStr10+=" ";
					localRecord->CustomStr10+=Marshal::PtrToStringAnsi((IntPtr)leaField);
				}
				//Set urlOk if attribute is resource
				if (cpr != nullptr && "resource" == leaAttribute && (localRecord->CustomStr10 != nullptr && localRecord->CustomStr10->Length > 0))
				{
					customData["urlOk"]=true;
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecCustomStr10] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecDateTime(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecDateTime");
				}
				time_t logtime;
				struct tm datetime;
				char timestring[21];

				logtime = (time_t) pRec->fields[leaFieldOrder].lea_time;
				int err;
				if (!(err=localtime_s (&datetime,&logtime))) {
					strftime (timestring, 21, "%Y/%m/%d %H:%M:%S", &datetime);
					localRecord->Datetime = Marshal::PtrToStringAnsi((IntPtr)timestring);
					return true;
				}
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecDateTime] localtime_s failed:"+err);
				}
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecDateTime] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecDescription(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecDescription");
				}

				int lD=localRecord->Description == nullptr ? 0 : localRecord->Description->Length;
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if (maxLen>=0 && lD > maxLen ) {
					localRecord->Description=localRecord->Description->Substring(0,maxLen);
				} else if (lD < maxLen) {

					char buffer[128];
					char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
					int lF=leaField == NULL ? 0 : (int)strlen(leaField);
					if (lF > 0) {
						if (lD > 0) {
							++lD;
							localRecord->Description+=" ";
						}

						if (maxLen >=0 && lD+lF > maxLen) {
							leaField[maxLen-lD]='\0';
						}
						localRecord->Description+=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
					}
					return true;
				}
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecDescription] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecURL(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecURL");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				if(leaField != NULL && *leaField) {
					String^ uriVal=Marshal::PtrToStringAnsi((IntPtr)leaField);

					if (log != nullptr) {
						log->Log(LogType::FILE, LogLevel::DEBUG, "setLea2RecURL: URI["+uriVal+"]");
					}
					array<int>^ maxLen=ctxArgs == nullptr ? nullptr : (array<int>^)ctxArgs;
					String^ customStr9=nullptr;
					String^ customStr10=nullptr;
					try {
						Uri^ uri=gcnew Uri(uriVal,UriKind::RelativeOrAbsolute);
						if (uri->IsAbsoluteUri)
						{
							customStr10=uri->PathAndQuery+uri->Fragment;
							if (customStr10 != nullptr && customStr10 != "/") {
								customStr9=uri->AbsoluteUri->Substring(0,uri->AbsoluteUri->Length-customStr10->Length);
							} else {
								customStr9=uri->AbsoluteUri;
								customStr10=nullptr;
							}
						} else {
							customStr10=uriVal;
						}
					}catch(Exception^ ue) {
						customStr9=uriVal;
						customStr10=nullptr;
					}
					if (maxLen != nullptr && maxLen->Length > 0) {
						if (customStr9 != nullptr && maxLen[0] >= 0 && customStr9->Length > maxLen[0]) {
							customStr9=customStr9->Substring(0,maxLen[0]);
						}
						if (maxLen->Length > 1) {
							if (customStr10 != nullptr && maxLen[1] >= 0 && customStr10->Length > maxLen[1]) {
								customStr10=customStr10->Substring(0,maxLen[1]);
							}
						}
					}
					localRecord->CustomStr9=customStr9;
					localRecord->CustomStr10=customStr10;
				} else {
					localRecord->CustomStr9=nullptr;
					localRecord->CustomStr10=nullptr;
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecURL] " + ex->Message+"\n" + ex->StackTrace );
				}
			}

			return false;
		}

		bool setLea2RecEventCategory(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecEventCategory");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->EventCategory=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecEventCategory] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		bool setLea2RecEventType(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecEventType");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->EventType=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecEventType] " + ex->Message+"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecNONE(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecNONE");
				}
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecNONE] " + ex->Message +"\n" + ex->StackTrace);
				}
			}
			return false;
		}

		bool setLea2RecSourceName(CustomTools::CustomBase::Rec^ localRecord,int leaFieldOrder, int leaAttributeId, String^ leaAttribute, OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[],Object^ ctxArgs, Log::CLogger^ log)
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Calling setLea2RecSourceName");
				}

				char buffer[128];
				char *leaField=ResolveField(pSession,&pRec->fields[leaFieldOrder],buffer);
				int maxLen=ctxArgs != nullptr ? Convert::ToInt32(ctxArgs) : -1;
				if(maxLen >=0 && leaField != NULL && (int)strlen(leaField) > maxLen)
				{
					leaField[maxLen]='\0';
				}
				localRecord->SourceName=leaField != NULL ? Marshal::PtrToStringAnsi((IntPtr)leaField) : nullptr;
				return true;
			} catch(Exception^ ex) {
				if(log != nullptr) {
					log->Log(Log::LogType::FILE, Log::LogLevel::ERROR, "[GPFN0000] [setLea2RecSourceName] " + ex->Message+"\n" + ex->StackTrace );
				}
			}
			return false;
		}

		void InitializeDictionary(Dictionary<String^, Lea2RecField^>^ LeaMappersLookup)
		{
			LeaMappersLookup["loc"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt10)); //#define LIDX_NUM 0
			LeaMappersLookup["time"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDateTime)); //#define LIDX_TIME 1
			LeaMappersLookup["action"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecEventCategory)); //#define LIDX_ACTION 2
			LeaMappersLookup["orig"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecComputerName)); //#define LIDX_ORIG 3
			LeaMappersLookup["alert"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr2)); //#define LIDX_ALERT 4
			LeaMappersLookup["i/f_dir"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr2)); //#define LIDX_IF_DIR 5
			LeaMappersLookup["i/f_name"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecSourceName)); //#define LIDX_IF_NAME 6
			LeaMappersLookup["has_accounting"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_HAS_ACCOUNTING 7
			LeaMappersLookup["uuid"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_UUID 8
			LeaMappersLookup["product"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr1)); //#define LIDX_PRODUCT 9
			LeaMappersLookup["__policy_id_tag"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_POLICY_ID_TAG 10
			LeaMappersLookup["src"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr3)); //#define LIDX_SRC 11
			LeaMappersLookup["s_port"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt1)); //#define LIDX_S_PORT 12
			LeaMappersLookup["dst"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr4)); //#define LIDX_DST 13
			LeaMappersLookup["service"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt2)); //#define LIDX_SERVICE 14
			LeaMappersLookup["tcp_flags"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_TCP_FLAGS 15
			LeaMappersLookup["proto"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr7)); //#define LIDX_PROTO 16
			LeaMappersLookup["rule"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr7)); //#define LIDX_RULE 17
			LeaMappersLookup["xlatesrc"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr5)); //#define LIDX_XLATESRC 18
			LeaMappersLookup["xlatedst"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr6)); //#define LIDX_XLATEDST 19
			LeaMappersLookup["xlatesport"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt4)); //#define LIDX_XLATESPORT 20
			LeaMappersLookup["xlatedport"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt5)); //#define LIDX_XLATEDPORT 21
			LeaMappersLookup["NAT_rulenum"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr7)); //#define LIDX_NAT_RULENUM 22
			LeaMappersLookup["resource"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_RESOURCE 23
			LeaMappersLookup["elapsed"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_ELAPSED 24
			LeaMappersLookup["packets"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_PACKETS 25
			LeaMappersLookup["bytes"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_BYTES 26
			LeaMappersLookup["reason"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_REASON 27
			LeaMappersLookup["service_name"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVICE_NAME 28
			LeaMappersLookup["agent"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_AGENT 29
			LeaMappersLookup["from"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr5)); //#define LIDX_FROM 30
			LeaMappersLookup["to"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr6)); //#define LIDX_TO 31
			LeaMappersLookup["sys_msgs"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_SYS_MSGS 32
			LeaMappersLookup["fw_message"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_FW_MESSAGE 33
			LeaMappersLookup["Internal_CA:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_INTERNAL_CA 34
			LeaMappersLookup["serial_num:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERIAL_NUM 35
			LeaMappersLookup["dn:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DN 36
			LeaMappersLookup["ICMP"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_ICMP 37
			LeaMappersLookup["icmp-type"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt1)); //#define LIDX_ICMP_TYPE 38
			LeaMappersLookup["ICMP Type"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt7)); //#define LIDX_ICMP_TYPE2 39
			LeaMappersLookup["icmp-code"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt2)); //#define LIDX_ICMP_CODE 40
			LeaMappersLookup["ICMP Code"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt8)); //#define LIDX_ICMP_CODE2 41
			LeaMappersLookup["msgid"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_MSGID 42
			LeaMappersLookup["message_info"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_MESSAGE_INFO 43
			LeaMappersLookup["log_sys_message"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_LOG_SYS_MESSAGE 44
			LeaMappersLookup["session_id:"]=gcnew Lea2RecField(-1,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomInt6)); //#define LIDX_SESSION_ID 45
			LeaMappersLookup["dns_query"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_DNS_QUERY 46
			LeaMappersLookup["dns_type"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_DNS_TYPE 47
			LeaMappersLookup["scheme:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_SCHEME 48
			LeaMappersLookup["srckeyid"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_SRCKEYID 49
			LeaMappersLookup["dstkeyid"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_DSTKEYID 50
			LeaMappersLookup["methods:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_METHODS 51
			LeaMappersLookup["peer gateway"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_PEER_GATEWAY 52
			LeaMappersLookup["IKE:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_IKE 53
			LeaMappersLookup["IKE IDs:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_IKE_IDS 54
			LeaMappersLookup["encryption failure:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_ENCRYPTION_FAILURE 55
			LeaMappersLookup["encryption fail reason:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_ENCRYPTION_FAIL_R 56
			LeaMappersLookup["CookieI"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_COOKIEI 57
			LeaMappersLookup["CookieR"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_COOKIER 58
			LeaMappersLookup["start_time"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_START_TIME 59
			LeaMappersLookup["segment_time"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SEGMENT_TIME 60
			LeaMappersLookup["client_inbound_packets"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_IN_PACKETS 61
			LeaMappersLookup["client_outbound_packets"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_OUT_PACKETS 62
			LeaMappersLookup["client_inbound_bytes"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_IN_BYTES 63
			LeaMappersLookup["client_outbound_bytes"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_OUT_BYTES 64
			LeaMappersLookup["client_inbound_interface"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_IN_IF 65
			LeaMappersLookup["client_outbound_interface"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLIENT_OUT_IF 66
			LeaMappersLookup["server_inbound_packets"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_IN_PACKETS 67
			LeaMappersLookup["server_outbound_packets"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_OUT_PACKETS 68
			LeaMappersLookup["server_inbound_bytes"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_IN_BYTES 69
			LeaMappersLookup["server_outbound_bytes"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_OUT_BYTES 70
			LeaMappersLookup["server_inbound_interface"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_IN_IF 71
			LeaMappersLookup["server_outbound_interface"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SERVER_OUT_IF 72
			LeaMappersLookup["message"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_MESSAGE 73
			LeaMappersLookup["NAT_addtnl_rulenum"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_NAT_ADDRULENUM 74
			LeaMappersLookup["user"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_USER 75
			LeaMappersLookup["srcname"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_SRCNAME 76
			LeaMappersLookup["vpn_user"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_VPN_USER 77
			LeaMappersLookup["OM:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_OM 78
			LeaMappersLookup["om_method:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_OM_METHOD 79
			LeaMappersLookup["assigned_IP:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_ASSIGNED_IP 80
			LeaMappersLookup["MAC:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_MAC 81
			LeaMappersLookup["attack"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_ATTACK 82
			LeaMappersLookup["Attack Info"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_ATTACK_INFO 83
			LeaMappersLookup["Cluster_Info"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CLUSTER_INFO 84
			LeaMappersLookup["DCE-RPC Interface UUID"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DCE_RPC_UUID 85
			LeaMappersLookup["DCE-RPC Interface UUID-1"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DCE_RPC_UUID_1 86
			LeaMappersLookup["DCE-RPC Interface UUID-2"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DCE_RPC_UUID_2 87
			LeaMappersLookup["DCE-RPC Interface UUID-3"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DCE_RPC_UUID_3 88
			LeaMappersLookup["during_sec"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DURING_SEC 89
			LeaMappersLookup["fragments_dropped"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_FRAGMENTS_DROPPED 90
			LeaMappersLookup["ip_id"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_IP_ID 91
			LeaMappersLookup["ip_len"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_IP_LEN 92
			LeaMappersLookup["ip_offset"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_IP_OFFSET 93
			LeaMappersLookup["TCP flags"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_TCP_FLAGS2 94
			LeaMappersLookup["sync_info:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_SYNC_INFO 95
			LeaMappersLookup["log"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_LOG 96
			LeaMappersLookup["cpmad"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_CPMAD 97
			LeaMappersLookup["auth_method"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr9)); //#define LIDX_AUTH_METHOD 98
			LeaMappersLookup["TCP packet out of state"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_TCP_PACKET_OOS 99
			LeaMappersLookup["rpc_prog"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_RPC_PROG 100
			LeaMappersLookup["th_flags"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_TH_FLAGS 101
			LeaMappersLookup["cp_message:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_CP_MESSAGE 102
			LeaMappersLookup["reject_category"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_REJECT_CATEGORY 103
			LeaMappersLookup["IKE Log:"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_IKE_LOG 104
			LeaMappersLookup["Negotiation Id:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_NEGOTIATION_ID 105
			LeaMappersLookup["decryption failure:"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_DECRYPTION_FAILURE 106
			LeaMappersLookup["len"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_LEN 107
			LeaMappersLookup["activity"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr2)); //#define LIDX_NEW_ACTIVITY 108
			LeaMappersLookup["Update Status"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecEventType)); //#define LIDX_NEW_UPDATE_STATUS 109
			LeaMappersLookup["update_src"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr5)); //#define LIDX_NEW_UPDATE_SRC 110
			LeaMappersLookup["sig_ver"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr4)); //#define LIDX_NEW_SIG_VER 111
			LeaMappersLookup["cvpn_category"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecNONE)); //#define LIDX_NEW_CVPN_CATEGORY 112
			LeaMappersLookup["group"]=gcnew Lea2RecField(4000,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecDescription)); //#define LIDX_NEW_GROUP 113
			LeaMappersLookup["ICS_scan_status"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecEventType)); //#define LIDX_NEW_ICS_SCAN_STATUS 114
			LeaMappersLookup["ICS_access_status"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecEventType)); //#define LIDX_NEW_ICS_ACCESS_STATUS 115
			LeaMappersLookup["spyware_name"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr7)); //#define LIDX_NEW_SPYWARE_NAME 116
			LeaMappersLookup["spyware_type"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr4)); //#define LIDX_NEW_SPYWARE_TYPE 117
			LeaMappersLookup["description"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr10)); //#define LIDX_NEW_DESCRIPTION 118
			LeaMappersLookup["access_status"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecEventType)); //#define LIDX_NEW_ACCESS_STATUS 119
			LeaMappersLookup["cvpn_resource"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr8)); //#define LIDX_NEW_CVPN_RESOURCE 120
			LeaMappersLookup["url"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr2)); //#define LIDX_NEW_URL 121
			LeaMappersLookup["outgoing_url"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr4)); //#define LIDX_NEW_OUTGOING_URL 122
			LeaMappersLookup["snid"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr7)); //#define LIDX_NEW_SNID 123
			LeaMappersLookup["dstname"]=gcnew Lea2RecField(900,gcnew Lea2RecFieldDelegate(this,&Parser::CheckPointRecorder::setLea2RecCustomStr6)); //#define LIDX_NEW_DSTNAME 124
		}

		virtual void SetConfigData(Int32 Identity, String^ LEA_opsec_sslca_file, String^ LEA_auth_type, String^ Position, 
			String^ LastFile, String^ LEA_auth_port, Boolean OldVersion, Int32 Maxrecord, String^ LEA_opsec_entity_sic_name, 
			String^ LEA_opsec_sic_name, String^ LEA_ip, Int32 OpsecDebugLevel, Int32 TraceLevel,              
			String^ Filename, Int32 OnlineMode, String^ virtualHost, String^ dal,Int32 Zone) override
		{
			if (log != nullptr) {
				log->Log(LogType::FILE, LogLevel::DEBUG, "SetConfigData Begins");
			}
			usingRegistry = FALSE;
			Id=Identity;
			fw1_2000 = OldVersion;
			online_mode = Convert::ToBoolean(OnlineMode);
			opsec_debug_level = OpsecDebugLevel;
			auth_type = LEA_auth_type;
			ip = LEA_ip;
			auth_port = LEA_auth_port;
			opsec_entity_sic_name = LEA_opsec_entity_sic_name;
			opsec_sic_name = LEA_opsec_sic_name;
			opsec_sslca_file = LEA_opsec_sslca_file;

			parametricFilename=Filename;
			ResetLogFilenameWith(parametricFilename);
			maxRecord=Maxrecord;
			eof=false;
			totalPass=0;
			VirtualHost = virtualHost;
			Dal = dal;
			log->SetLogLevel((LogLevel)TraceLevel);
			if (log != nullptr) {
				log->Log(LogType::FILE, LogLevel::DEBUG, "SetConfigData Ends");
			}
		}

		bool ResetLogFilenameWith(String^ Filename) 
		{
			fileId=0;

			log->Log(LogType::FILE, LogLevel::DEBUG, "ResetLogFilenameWith:["+Filename+"]");
			if (Filename != nullptr && Filename->Length > 0) {
				try {
					String^ delimStr = ",";
					array<Char>^ delimiter = delimStr->ToCharArray();
					array<String^>^ fs=Filename->Split(delimiter,4);

					if (fs != nullptr && (fs->Length == 3 || fs->Length == 4)) {
						fileId=long::Parse(fs[0]);
						//creationTime=long::Parse(fs[1]);
						filename=fs[fs->Length-2];
						needLastPositionReset=int::Parse(fs[fs->Length-1]) > 0;
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "ResetLogFilenameWith ERROR: ["+Filename+"]: Invalid");
						return false;
					}
				}catch(Exception^ fe) {
					log->Log(LogType::FILE, LogLevel::ERROR, "ResetLogFilenameWith EXCEPTION: ["+Filename+"]:"+fe->Message+"\n" + fe->StackTrace);
					return false;
				}
			} else {
				filename=nullptr;
				needLastPositionReset=false;
			}


			if (needLastPositionReset) {
				log->Log(LogType::FILE, LogLevel::DEBUG, "ResetLogFilenameWith: needLastPositionReset");
				if (SetLastPosition(0)) {
					String^ newFilename=fileId+","+filename+",0";
					log->Log(LogType::FILE, LogLevel::DEBUG, "ResetLogFilenameWith: needLastPositionReset SetLastPosition=>["+newFilename+"]");
					if (!SetFilename(newFilename)) {
						log->Log(LogType::FILE, LogLevel::DEBUG, "ResetLogFilenameWith: needLastPositionReset FAIL SetFilename:["+newFilename+"]");
						return false;
					}
				} else {
					log->Log(LogType::FILE, LogLevel::DEBUG, "ResetLogFilenameWith: needLastPositionReset FAIL SetLastPosition");
					return false;
				}
			}
			return true;
		}

		bool SetLastParams(int lastPosition,String^ lastRecordDateTime)
		{
			log->Log(LogType::FILE, LogLevel::DEBUG,"SetLastParams Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						reg->SetValue("LastPosition", lastPosition);
						lastPosition=lastPosition;

						reg->SetValue("LastRecord",lastRecordDateTime);
						lastRecordDateTime=lastRecordDateTime;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastParams EXCEPTION: REGISTRY NULL");
				} else {
					CustomServiceBase ^ser = getBaseService();
					if (ser != nullptr) {
						ser->SetReg(Id,
							lastPosition.ToString(),lastPosition.ToString(),
							parametricFilename,parametricFilename,
							lastRecordDateTime);
						lastPosition=lastPosition;
						lastRecordDateTime=lastRecordDateTime;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastParams EXCEPTION: BASE SERVICE NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "SetLastParams EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return false;
		}

		bool SetLastPosition(int lastPosition)
		{		
			log->Log(LogType::FILE, LogLevel::DEBUG,"SetLastPosition Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						reg->SetValue("LastPosition", lastPosition);
						lastPosition=lastPosition;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastPosition EXCEPTION: REGISTRY NULL");
				} else {
					CustomServiceBase ^ser = getBaseService();
					if (ser != nullptr) {
						String^ filename=fileId+","+filename+",0";
						ser->SetReg(Id,
							lastPosition.ToString(),lastPosition.ToString(),
							parametricFilename,parametricFilename,
							lastRecordDateTime);
						lastPosition=lastPosition;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastPosition EXCEPTION: BASE SERVICE NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "SetLastPosition EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			//CheckPointRecorder::BreakPoint("Setlast failed");
			return false;
		}

		bool SetLastRecordDateTime(String^ lastRecordDateTime)
		{		
			log->Log(LogType::FILE, LogLevel::DEBUG,"SetLastRecordDateTime Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						reg->SetValue("LastRecord",lastRecordDateTime);
						lastRecordDateTime=lastRecordDateTime;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastRecordDateTime EXCEPTION: REGISTRY NULL");
				} else {
					CustomServiceBase ^ser = getBaseService();
					if (ser != nullptr) {
						ser->SetReg(Id,
							lastPosition.ToString(),lastPosition.ToString(),
							parametricFilename,parametricFilename,
							lastRecordDateTime);
						lastRecordDateTime=lastRecordDateTime;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetLastRecordDateTime EXCEPTION: BASE SERVICE NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "SetLastRecordDateTime EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return false;
		}

		bool SetFilename(String^ filename)
		{
			log->Log(LogType::FILE, LogLevel::DEBUG,"SetFilename Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						reg->SetValue("Filename",filename);
						parametricFilename=filename;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetFilename EXCEPTION: REGISTRY NULL");
				} else {
					CustomServiceBase ^ser = getBaseService();
					if (ser != nullptr) {
						ser->SetReg(Id,
							lastPosition.ToString(),lastPosition.ToString(),
							filename,filename,
							lastRecordDateTime);
						parametricFilename=filename;
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SetFilename EXCEPTION: BASE SERVICE NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "SetFilename EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return false;
		}

		int RefreshTimeout() {
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshTimeout Begins");
			try {
				if (reg != nullptr) {
					String^ timeout=(String^)reg->GetValue("SessionTimeout",nullptr);

					if (timeout != nullptr) {
						return long::Parse(timeout);
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "SessionTimeout EXCEPTION: SessionTimeout key value is NULL");
				} else {
					log->Log(LogType::FILE, LogLevel::ERROR, "RefreshTimeout EXCEPTION: REGISTRY NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "SessionTimeout EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return 0;
		}

		bool RefreshUseCollectedFiles() {
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshUseCollectedFiles Begins");
			try {
				if (reg != nullptr) {
					useCollectedFiles=(int)reg->GetValue("UseCollectedFiles",nullptr);	
					return true;
				}
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshUseCollectedFiles EXCEPTION: REGISTRY NULL");
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshUseCollectedFiles EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return 0;
		}


		bool RefreshStopOnFileId() {
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshStopOnFileId Begins");
			try {
				if(isolatedRegistry != nullptr)
				{
					if (reg != nullptr) {
						String^ fileId=(String^)reg->GetValue("Stop On File Id",nullptr);

						log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshStopOnFileId:["+fileId+"]");
						if (fileId != nullptr) {
							stopOnFileId=long::Parse(fileId);
							return true;
						} else {
							log->Log(LogType::FILE, LogLevel::ERROR, "RefreshStopOnFileId EXCEPTION: Stop On File Id key value is NULL");
						}
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "RefreshStopOnFileId EXCEPTION: REGISTRY NULL");
					}
				} else {
					stopOnFileId=long::MinValue;
					log->Log(LogType::FILE, LogLevel::ERROR, "RefreshStopOnFileId EXCEPTION: Can be called in isolated registry mode");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshStopOnFileId EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshStopOnFileId FAILED. Return false");
			return false;
		}

		bool RefreshGoOnWithServerReply() {
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshGoOnWithServerReply Begins");
			try {
				if (reg != nullptr) {
					int value=(int)reg->GetValue("Go On With Server Reply",nullptr);

					log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshGoOnWithServerReply:["+value+"]");
					goOnWithServerReply= value;
					return true;
				} else {
					log->Log(LogType::FILE, LogLevel::ERROR, "RefreshGoOnWithServerReply EXCEPTION: REGISTRY NULL");
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshGoOnWithServerReply EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshGoOnWithServerReply FAILED. Return false");
			return false;
		}

		bool RefreshLastFilename()
		{		
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshLastFilename Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						parametricFilename=(String^)reg->GetValue("Filename",nullptr);
						log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshLastFilename:ResetLogFileNameWith:["+parametricFilename+"]");
						if (ResetLogFilenameWith(parametricFilename)) {
							return true;
						}
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "RefreshLastFilename EXCEPTION: REGISTRY NULL");
					}
				} else {
					return true;
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshLastFilename EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshLastFilename FAILED. Return false");
			return false;
		}

		bool RefreshLastPosition()
		{		
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshLastPosition Begins");
			try {
				if(usingRegistry)
				{
					if (reg != nullptr) {
						lastPosition=(int)reg->GetValue("LastPosition");
						return true;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "RefreshLastPosition EXCEPTION: REGISTRY NULL");
				} else {
					return true;
				}
			} catch(Exception^ ex) {
				log->Log(LogType::FILE, LogLevel::ERROR, "RefreshLastPosition EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
			}
			return false;
		}

		void RefreshMaxRecord()
		{
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshMaxRecord Begins");
			if(usingRegistry && reg != nullptr)
			{
				try {
					maxRecord=(int)reg->GetValue("MaxRecord",Int32::MaxValue);
					return;
				}
				catch(Exception^ ex)
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "Unable to get MaxRecord: "+ex->Message+"\n" + ex->StackTrace);
				}
			}
			maxRecord=int::MaxValue;
		}

		void RefreshMaxLogFiles()
		{
			log->Log(LogType::FILE, LogLevel::DEBUG,"RefreshMaxLogFiles Begins");
			if(usingRegistry && reg != nullptr)
			{
				try {
					maxLogFiles=(int)reg->GetValue("TotalLogFiles",1);
					return;
				}
				catch(Exception^ ex)
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "Unable to get TotalLogFiles: "+ex->Message+"\n" + ex->StackTrace);
				}
			}
			maxLogFiles=1;
		}

		virtual void Start() override
		{
			try {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "Start BEGINs");
				}
				// Check if already running. If an instance found, just return
				Monitor::Enter(syncRoot);
				try {
					if (status != 0) {
						if (log != nullptr) {
							log->Log(LogType::FILE, LogLevel::DEBUG, "Start Already running instance. RETURN");
						}
						return;
					}
					if (log != nullptr) {
						log->Log(LogType::FILE, LogLevel::DEBUG, "Start No Running Instance. do OPERATION");
					}
					status=1;
				} finally {
					Monitor::Exit(syncRoot);
				}
				try {
					totalPass=0;
					totalProcessed=0;
					recoveryMode=0;

					log->Log(LogType::FILE, LogLevel::DEBUG,"Start Params");

					while(!StartHandler() && totalProcessed < maxRecord && totalPass < maxLogFiles) 
					{
					}

					log->Log(LogType::FILE, LogLevel::DEBUG,"Start Ends TP="+totalProcessed
						+" MaxRecord="+maxRecord
						+" TotalPass="+totalPass
						+" MaxLogFiles="+maxLogFiles);
				} finally {
					Monitor::Enter(syncRoot);
					try {
						status=0;
					}finally {
						Monitor::Exit(syncRoot);
					}
				}
			}catch(Exception^ ex) {
				if (log != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "Start Exception:"+ex->Message+"\n"+ex->StackTrace);
				}
				//CheckPointRecorder::BreakPoint("Start EXCEPTION: "+ex->Message+"\n"+ex->StackTrace);
			}
		}

		int InitEnv([Out] String^% loc)
		{
			try {
				if (isolatedRegistry != nullptr) {
					reg = Registry::LocalMachine->OpenSubKey(isolatedRegistry, true);
					if(reg != nullptr)
					{
						try
						{
							loc = (String^)reg->GetValue("Home Directory");
							log->SetLogFile(loc + "\\log\\CheckPointRecorder.log");
							return 0;
						}
						catch(Exception^ ex)
						{
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Cannot read agent home directory in isolated mode:"
								+ex->Message+"\n" + ex->StackTrace);
						} finally {
							try {
								reg->Close();
							}catch(Exception^ re) {
							}
							reg=nullptr;
						}
					} else {
						try {
							log->Log(LogType::FILE, LogLevel::ERROR, "InitEnv: Agent No Registry isolated: "+isolatedRegistry);
						}catch(Exception^ e1) {
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Agent No Registry isolated: "+isolatedRegistry);
						}
					}
				} else if (usingRegistry)
				{
					reg = Registry::LocalMachine->OpenSubKey("SOFTWARE\\Natek\\Security Manager\\Agent", true);
					if(reg != nullptr)
					{
						try
						{
							log->Log(LogType::FILE, LogLevel::DEBUG, "Check SubRecorderId: ["+subRecorderId+"]");
							loc = (String^)reg->GetValue("Home Directory");
							if (!String::Equals(subRecorderId,""))
								log->SetLogFile(loc + "log\\CheckPointRecorder_"+subRecorderId+".log");
							else
								log->SetLogFile(loc + "log\\CheckPointRecorder.log");
							log->Log(LogType::FILE, LogLevel::DEBUG, "Check SubRecorderId 2: ["+subRecorderId+"]");
							return 0;
						}
						catch(Exception^ ex)
						{
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Cannot read agent home directory:"
								+ex->Message+"\n" + ex->StackTrace);
						} finally {
							try {
								reg->Close();
							}catch(Exception^ re) {
							}
							reg=nullptr;
						}
					} else {
						try {
							log->Log(LogType::FILE, LogLevel::ERROR, "InitEnv: Agent No Registry: SOFTWARE\\Natek\\Security Manager\\Agent");
						}catch(Exception^ e1) {
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Agent No Registry: SOFTWARE\\Natek\\Security Manager\\Agent");
						}
					}
				}
				else
				{
					reg = Registry::LocalMachine->OpenSubKey("Software\\NATEK\\Security Manager\\Remote Recorder", true);
					if(reg != nullptr)
					{
						try
						{
							loc = (String^)reg->GetValue("Home Directory");
							log->SetLogFile(loc + "log\\CheckPointRecorder.log");
							opsec_sslca_file = loc + "bin\\" + opsec_sslca_file;
							return 0;
						}
						catch(Exception^ ex)
						{
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Cannot read agent home directory:"+
								ex->Message+"\n" + ex->StackTrace);
						}finally {
							try {
								reg->Close();
							}catch(Exception^ re) {}
							reg=nullptr;
						}
					} else {
						try {
							log->Log(LogType::FILE, LogLevel::ERROR, "InitEnv: Agent No Registry: Software\\NATEK\\Security Manager\\Remote Recorder");
						}catch(Exception^ e1) {
							log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv: Agent No Registry: Software\\NATEK\\Security Manager\\Remote Recorder");
						}
					}
				}
			}catch(Exception^ ex) {
				try {
					log->Log(LogType::FILE, LogLevel::ERROR, "InitEnv EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
				}catch(Exception^ te) {
					log->Log(LogType::EVENTLOG, LogLevel::ERROR, "InitEnv EXCEPTION:"+ex->Message+"\n" + ex->StackTrace);
				}
			}
			return 1;
		}

		void InitOpsecParamsReg(String^ loc) {
			fw1_2000 = Convert::ToInt32(reg->GetValue("OldVersion")) > 0;
			online_mode = Convert::ToInt32(reg->GetValue("OnlineMode")) > 0;
			opsec_debug_level = Convert::ToInt32(reg->GetValue("OpsecDebugLevel"));
			auth_type = Convert::ToString(reg->GetValue("LEA_auth_type"));
			ip = Convert::ToString(reg->GetValue("LEA_ip"));
			auth_port = Convert::ToString(reg->GetValue("LEA_auth_port"));
			opsec_entity_sic_name = Convert::ToString(reg->GetValue("LEA_opsec_entity_sic_name"));
			opsec_sic_name = Convert::ToString(reg->GetValue("opsec_sic_name"));
			opsec_sslca_file = Convert::ToString(reg->GetValue("opsec_sslca_file"));
			//Opsec filename fix
			opsec_sslca_file = loc + "\\bin\\" + opsec_sslca_file;
			log->SetLogLevel((LogLevel)Convert::ToInt32(reg->GetValue("Trace Level")));
		}

		int CreateSubRecorders()
		{
			array<String^>^ arr=reg->GetSubKeyNames();
			subRecorders->Clear();
			for each(String^ name in arr) {
				RegistryKey^ sub=nullptr;
				try {
					sub=reg->OpenSubKey(name);
					if (sub == nullptr)
						continue;
					String^ enabled=(String^)sub->GetValue("Enabled");
					if (enabled == nullptr || enabled == "0" || enabled == "")
						continue;
					CheckPointRecorder^ newRec;
					subRecorders[name]=newRec=gcnew CheckPointRecorder(nullptr,name);
					newRec->CreateList(GetInstanceListService(), GetInstanceListBase());
					newRec->VirtualHost=this->VirtualHost;
					newRec->Dal=this->Dal;
					newRec->Id=this->Id;
				} finally {
					if (sub != nullptr) {
						sub->Close();
					}
				}
			}
			RunSubRecorders();
			return 0;
		}

		void RunSubRecorders() {
			if (subRecorders->Count == 0)
				return;

			for each(String^ key in subRecorders->Keys) {
				CheckPointRecorder^ rec=subRecorders[key];
				rec->thread=gcnew Thread(gcnew System::Threading::ThreadStart(rec,&Parser::CheckPointRecorder::Start));
				try {
					rec->thread->Start();
				}catch(Exception^ ex) {
					rec->thread=nullptr;
				}
			}
		}

		int InitOpsecParams(String^ loc)
		{
			try {
				if (isolatedRegistry != nullptr) {
					reg = Registry::LocalMachine->OpenSubKey(isolatedRegistry, true);
					if(reg != nullptr)
					{
						InitOpsecParamsReg(loc);
						return 0;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "InitOpsecParams: Agent No Registry isolated: "+isolatedRegistry);
				} else if(usingRegistry)
				{
					reg = Registry::LocalMachine->OpenSubKey("SOFTWARE\\Natek\\Security Manager\\Recorder\\CheckPointRecorder"
						+(String::Equals(subRecorderId,"") ? subRecorderId : "\\"+subRecorderId), true);
					if(reg != nullptr)
					{
						if (String::Equals(subRecorderId,"")) {
							log->Log(LogType::FILE, LogLevel::DEBUG, "InitOpsecParams: Check Mode");
							String^ mode=(String^)reg->GetValue("Mode");
							isMultiMode=mode != nullptr && String::Compare(mode,"multi",System::StringComparison::InvariantCultureIgnoreCase) == 0;
							if (isMultiMode) {
								log->Log(LogType::FILE, LogLevel::DEBUG, "InitOpsecParams: Multi Mode, So Create Sub Recorders");
								return CreateSubRecorders();
							} else {
								log->Log(LogType::FILE, LogLevel::DEBUG, "InitOpsecParams: Single Mode Just Run As Usual");
							}
						} else {
							log->Log(LogType::FILE, LogLevel::DEBUG, "InitOpsecParams: Sub Recorder Call InitOpsecParamsReg:"+subRecorderId);
						}
						InitOpsecParamsReg(loc);
						return 0;
					}
					log->Log(LogType::FILE, LogLevel::ERROR, "InitOpsecParams: Agent No Registry: SOFTWARE\\Natek\\Security Manager\\Recorder\\CheckPointRecorder");
				} else {
					return 0;
				}
			}
			catch(Exception^ ex)
			{
				log->Log(LogType::FILE, LogLevel::ERROR, "InitOpsecParams EXCEPTION:"+ex->Message + ", \n" + ex->StackTrace+"\n" + ex->StackTrace);
			}
			return 1;
		}

		void FreeMemToFree(array<IntPtr>^% memToFree)
		{
			if (memToFree != nullptr) {
				for(int i=0; i < memToFree->Length;i++) {
					if (memToFree[i] != IntPtr::Zero) {
						Marshal::FreeHGlobal(memToFree[i]);
					}
				}
				memToFree=nullptr;
			}
		}

		int InitRunArgs(char ***arr, array<IntPtr>^% memToFree)
		{
			*arr= NULL;
			memToFree=nullptr;
			try {
				*arr=new char*[CONFIG_LENGTH];

#define LOCAL_MEM_TO_FREE 6

				memToFree=gcnew array<IntPtr>(LOCAL_MEM_TO_FREE);

				(*arr)[0] = "-v";
				(*arr)[1] = "lea_server";
				(*arr)[2] = "auth_type";
				memToFree[0]=Marshal::StringToHGlobalAnsi(auth_type);
				(*arr)[3] = (char*)memToFree[0].ToPointer();

				(*arr)[4] = "-v";
				(*arr)[5] = "lea_server";
				(*arr)[6] = "ip";
				memToFree[1]=Marshal::StringToHGlobalAnsi(ip);
				(*arr)[7]=(char*)memToFree[1].ToPointer();
				log->Log(Log::LogType::FILE,LogLevel::INFORM,"lea_server ip:"+ip+":"+memToFree[1].ToInt64());

				(*arr)[8] = "-v";
				(*arr)[9] = "lea_server";
				(*arr)[10] = "auth_port";
				memToFree[2]=Marshal::StringToHGlobalAnsi(auth_port);
				(*arr)[11]=(char*)memToFree[2].ToPointer();

				(*arr)[12] = "-v";
				(*arr)[13] = "opsec_sic_name";
				memToFree[3]=Marshal::StringToHGlobalAnsi(opsec_sic_name);
				(*arr)[14]=(char*)memToFree[3].ToPointer();

				(*arr)[15] = "-v";
				(*arr)[16] = "opsec_sslca_file";
				memToFree[4]=Marshal::StringToHGlobalAnsi(opsec_sslca_file);
				(*arr)[17]=(char*)memToFree[4].ToPointer();

				(*arr)[18] = "-v";
				(*arr)[19] = "lea_server";
				(*arr)[20] = "opsec_entity_sic_name";
				memToFree[5]=Marshal::StringToHGlobalAnsi(opsec_entity_sic_name);
				(*arr)[21]=(char*)memToFree[5].ToPointer();
				return 0;
			}catch(Exception^ ex) {
				if (arr != NULL) {
					try {
						delete[] arr;
					} catch(Exception^ ae) {}
					arr=NULL;
				}
				FreeMemToFree(memToFree);
				log->Log(LogType::FILE, LogLevel::ERROR, "InitRunArgs EXCEPTION:"+ex->Message + ", \n" + ex->StackTrace+"\n" + ex->StackTrace);
			}
			return 1;
		}

		void WaitSubProcessors() {
			if (subRecorders->Count == 0)
				return;
			for each(CheckPointRecorder^ rec in subRecorders->Values) {
				if (rec->thread != nullptr) {
					try {
						rec->thread->Join();
					}catch(Exception^ ex) {
						log->Log(LogType::FILE, LogLevel::ERROR, "WaitSubProcessors Join EXCEPTION:"+ex->Message + ", \n" + ex->StackTrace);
					}
					finally {
						log->Log(LogType::FILE, LogLevel::DEBUG, "WaitSubProcessors Join END For:"+rec->subRecorderId);
					}
				}
			}
		}

		int StartHandler()
		{
			try
			{
				log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler Begins");
				String^ loc="";

				log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitEnv");
				if (InitEnv(loc)) {
					log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitEnv Failed");
					return 1;
				}

				log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitOpsecParams");
				if (InitOpsecParams(loc)) {
					log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitOpsecParams Failed");
					return 1;
				} else if (String::Equals(subRecorderId,"") && isMultiMode) {
					WaitSubProcessors();
					return 1;
				}

				char** arr=NULL;
				try {
					log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitRunArgs:["+subRecorderId+"] "+isMultiMode+":"+String::Equals(subRecorderId,""));
					("init run args");

					if (!InitRunArgs(&arr,memToFree))
					{
						log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->Refresh Params");
						if (RefreshLastFilename()
							&& RefreshLastPosition()) {
								log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->Refresh Inner Params");
								RefreshMaxRecord();
								RefreshMaxLogFiles();
								log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->run");
								return run(CONFIG_LENGTH,arr);
						} else {
							log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->Refresh Inner Params Failed");
						}
					} else {
						log->Log(LogType::FILE, LogLevel::DEBUG,"StartHandler->InitRunArgs Failed");
					}
				} finally {
					if (arr != NULL) {
						delete[] arr;
						arr=NULL;
					}
					FreeMemToFree(memToFree);
				}
			}
			catch(Exception^ ex)
			{
				log->Log(LogType::EVENTLOG, LogLevel::ERROR, ex->Message + ", \n" + ex->StackTrace);
			}
			return 1;
		}

		CheckPointRecorder^ getInstance()
		{
			return this;
		}

		CustomServiceBase^ getBaseService()
		{
			return usingRegistry 
				? GetInstanceService("Security Manager Sender")
				: GetInstanceService("Security Manager Remote Recorder");
		}

		void SetSendData(Rec^ r)
		{
			log->Log(LogType::FILE, LogLevel::DEBUG,"Sending Record Data Getting service: "+usingRegistry);	
			CustomServiceBase ^ser = getBaseService();
			log->Log(LogType::FILE, LogLevel::DEBUG,"Sending Record Data: "+(ser == nullptr));	
			if(usingRegistry)
			{
				ser->SetData(r);
			}
			else
			{
				ser->SetData(Dal, VirtualHost, r);
			}

			log->Log(LogType::FILE, LogLevel::DEBUG,"Finishing Record Data");
		}

		void RecoveryModeInNormal(OpsecSession *psession,Parser::CheckPointRecorder^ cpr)
		{
			int fileId,fileId_best;
			int accountFileId;
			char *filename;
			String^ filename_best;
			//CheckPointRecorder::BreakPoint("SessionFilematch: Need file seek");
			fileId_best=int::MaxValue;

			int r;

			//CheckPointRecorder::BreakPoint("BEGIN SEEK");
			if ((r=lea_get_first_file_info(psession,&filename,&fileId,&accountFileId)) != LEA_SESSION_IT_END) 
			{
				do 
				{
					if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
						return;
					}
					if (fileId >= this->fileId
						&& fileId <= fileId_best) 
					{
						String^ tmpFile=Marshal::PtrToStringAnsi((IntPtr)filename);
						if (String::Equals(tmpFile,"purged",System::StringComparison::InvariantCultureIgnoreCase))
							continue;
						fileId_best=fileId;
						filename_best=tmpFile;
					}
				} while((r=lea_get_next_file_info(psession,&filename,&fileId,&accountFileId)) != LEA_SESSION_IT_END);

				if (r == LEA_SESSION_IT_END
					&& fileId_best != int::MaxValue) 
				{
					String^ newFilename=fileId_best+","+filename_best+ "," + (fileId_best == this->fileId ? "0" : "1");
					if (SetFilename(newFilename)) {
						if (ResetLogFilenameWith(newFilename))
						{
							recoveryMode=255; //Recovery done successfully
							//Will return 1 since operation should start again
						} else {
							log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInNormal: ResetLogFilenameWith failed:"+newFilename);
						}
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInNormal: setfilename failed:"+newFilename);
					}
				} else {
					log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInNormal: NO best file found");
				}
			} else {
				log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInNormal: No best found first, getting first file failed");
			}
		}

		void RecoveryModeInCollected(OpsecSession *psession,Parser::CheckPointRecorder^ cpr)
		{
			collected_file_info_t *info;
			lea_value_ex_t *fileid_ex,*filename_ex;
			long fileId,fileId_best;
			int actual_filenameLen;
			char filename_buff[8192];
			String^ filename_best;
			int r;

			if ((r=lea_get_first_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL) 
			{
				fileId_best=LONG::MinValue;

				do {
					if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
						log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: first level session error");
						return;
					}

					if ((fileid_ex=lea_get_collected_file_info_by_field_name(info,"FileId")) == NULL
						|| lea_value_ex_get(fileid_ex,&fileId) != OPSEC_SESSION_OK) {
							log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: getting file id failed");
							return;
					}
					if (fileId >= this->fileId
						&& fileId <= fileId_best) 
					{
						if ((filename_ex=lea_get_collected_file_info_by_field_name(info,"Log Filename")) == NULL
							|| lea_value_ex_get(filename_ex,filename_buff,8192,&actual_filenameLen) != OPSEC_SESSION_OK) {
								log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: getting filename failed");
								return;
						}
						String^ tmpFile=Marshal::PtrToStringAnsi((IntPtr)filename_buff);
						if (String::Equals(tmpFile,"purged",System::StringComparison::InvariantCultureIgnoreCase))
							continue;
						fileId_best=fileId;
						filename_best=tmpFile;
					}
				} while((r=lea_get_next_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL);

				if (r == LEA_SESSION_IT_END
					&& fileId_best != int::MaxValue) 
				{
					String^ newFilename=fileId_best+","+filename_best+ "," + (fileId_best == this->fileId ? "0" : "1");
					if (SetFilename(newFilename)) {
						if (ResetLogFilenameWith(newFilename))
						{
							recoveryMode=255; //Recovery done successfully
							//Will return 1 since operation should start again
						} else {
							log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: ResetLogFilenameWith failed:"+newFilename);
						}
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: setfilename failed:"+newFilename);
					}
				} else {
					log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: NO best file found");
				}
			} else {
				log->Log(LogType::FILE, LogLevel::ERROR, "RecoveryModeInCollected: No best found first, getting first file failed");
			}
		}

		int SessionFileMatch(OpsecSession *psession,Parser::CheckPointRecorder^ cpr,bool checkCurrentLogFile) 
		{
			try {
				log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: Begin");
				if (recoveryMode == 0)
				{
					log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: not recovery Mode");
					log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: FileId("+fileId+"), Filename("+filename+")");
					if (fileId > 0 && filename != nullptr && filename->Length > 0) {
						//CheckPointRecorder::BreakPoint("SessionFilematch: No file check required");
						if (!checkCurrentLogFile) {
							log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: not check current, return");
							return 0;
						}

						log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: checking current, get desc");
						lea_logdesc * curr=lea_get_logfile_desc(psession);
						if (curr == nullptr) {
							log->Log(LogType::FILE, LogLevel::WARN, "SessionFileMatch: Failed to get current logfile desc");
							return 1;
						}
						log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: Cehck current and fileId");
						log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: Curr("+curr->fileid+"), FileId("+fileId+")");

						if (curr->fileid == fileId) {
							log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: equal, return 0");
							return 0;
						}
						log->Log(LogType::FILE, LogLevel::DEBUG, "SessionFileMatch: Check refresh go on server reply");
						if (RefreshGoOnWithServerReply()) {
							String^ filename=Marshal::PtrToStringAnsi((IntPtr)curr->filename);
							if (goOnWithServerReply > 0) {
								filename=(fileId+1)+",natek_recovery_mode,1";
								if (ResetLogFilenameWith(filename)) {
									fileId=fileId+1;
									filename="natek_recovery_mode";
									log->Log(LogType::FILE, LogLevel::INFORM, "SessionFileMatch: File Reset to Server Reply File: ["+filename+"]");
								} else {
									return 1;
								}
							} else {
								log->Log(LogType::FILE, LogLevel::WARN, "SessionFileMatch: FileId MISMATCH. Expected ("+fileId+","+filename+") Found("+curr->fileid+","+filename+"). Going to Recovery Mode");
								return 1;
							}
						} else {
							return 1;
						}
					} else {
						log->Log(LogType::FILE, LogLevel::WARN, "SessionFileMatch: No File Id or Filename, going to recovery mode");
					}
					recoveryMode=1;
				}

				if (!RefreshUseCollectedFiles()) {
					useCollectedFiles=0;
					log->Log(LogType::FILE, LogLevel::INFORM, "SessionFileMatch: RefreshUseCollectedFiles failed. So do default, don't use collected files");
				}
				if (useCollectedFiles == 0)
					RecoveryModeInNormal(psession,cpr);
				else
					RecoveryModeInCollected(psession,cpr);

			}catch(Exception^ ex) {
				//CheckPointRecorder::BreakPoint("File seek exception"+ex->Message+"\n"+ex->StackTrace);
				log->Log(LogType::FILE, LogLevel::ERROR, "Exception in SessionFileMatch: "+ex->Message+"\n"+ex->StackTrace+"\n");
			}
			return 1;
		}

		int read_fw1_logfile_record (OpsecSession * pSession, lea_record * pRec, int pnAttribPerm[])
		{
			log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: Begin");
			Parser::CheckPointRecorder^ cpr=nullptr;
			try {
				cpr= Parser::CheckPointRecorder::getInstance();

				log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: Instance null?="+(cpr ==  nullptr));
				if (!fileCheckComplete) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: SEssion file match");
					if (SessionFileMatch(pSession,cpr,true))
					{
						return OPSEC_SESSION_ERR;
					}
					fileCheckComplete=true;
				}

				log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: new rec");
				CustomTools::CustomBase::Rec^ r = gcnew CustomBase::Rec();

				log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: get pos");
				int nCurrPosition = lea_get_record_pos (pSession);

				log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: Curr:"+nCurrPosition);
				log->Log(LogType::FILE, LogLevel::DEBUG, "read_fw1_logfile_record: LastPos:"+lastPosition);
				if(nCurrPosition <= lastPosition) {
					return OPSEC_SESSION_OK;
				}

				StreamWriter^ sw=nullptr;
				if (outputFile2 != nullptr) {
					try {
						sw=gcnew StreamWriter(outputFile2,true);
						sw->WriteLine("==========================");
					}catch(Exception ^we)
					{
						sw=nullptr;
					}
				}

				try {
					customData["urlOk"]=false;
					for (int i = 0; i < pRec->n_fields; i++)
					{
						char *attr=lea_attr_name (pSession, pRec->fields[i].lea_attr_id);
						if (attr != NULL) {
							if (outputFile2 != nullptr) {
								try {
									sw->Write("{0}=",Marshal::PtrToStringAnsi((IntPtr)attr));

									wchar_t *outStr=NULL;
									int wlen;
									char buffer[128];
									char * v=ResolveField(pSession,&pRec->fields[i],buffer);
									if (v != NULL) {
										sw->Write(" V={0} ",Marshal::PtrToStringAnsi((IntPtr)v));
									}
									v=lea_resolve_field(pSession,pRec->fields[i]);
									if (v != NULL) {
										sw->Write(" F={0} ",Marshal::PtrToStringAnsi((IntPtr)v));
									}
									sw->WriteLine(" T={0}",pRec->fields[i].lea_val_type);
								} catch(Exception^ we) {}
							}

							String^ attrName=Marshal::PtrToStringAnsi((IntPtr)attr);
							Lea2RecField^ fdel=nullptr;
							if (LeaMappersLookup->TryGetValue(attrName,fdel)
								&& fdel != nullptr && fdel->lea2RecFieldDelegate != nullptr) {
									try {
										fdel->lea2RecFieldDelegate(r,i,pRec->fields[i].lea_attr_id,attrName,pSession,pRec,pnAttribPerm,fdel->ctxArgs,log);
									}catch(Exception^ fe) {
										if (cpr != nullptr) {
											log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_record: call set attribute for:"+attrName);
										}
									}
							}
						}
					}
				}finally {
					if (sw != nullptr) {
						try {
							sw->Close();
						}catch(Exception^ swe) {}
					}
				}

				r->LogName = "CheckPointRecorder";
				r->Recordnum=lastPosition+1;

				if (isolatedRegistry != nullptr) {
					if (outputFile != nullptr) {
						StreamWriter^ sw=nullptr;
						try {
							sw=gcnew StreamWriter(outputFile,true);

							sw->WriteLine("[{0}] [{1}] [{2}] [{3}] [{4}] [{5}]"
								+" [{6}] [{7}] [{8}] [{9}] [{10}] [{11}]"
								+" [{12}] [{13}] [{14}] [{15}] [{16}] [{17}]"
								+" [{18}] [{19}] [{20}] [{21}] [{22}] [{23}]"
								+" [{24}] [{25}] [{26}] [{27}] [{28}] [{29}]",
								r->ComputerName,
								r->CustomInt1,r->CustomInt2,r->CustomInt3,r->CustomInt4,r->CustomInt5,
								r->CustomInt6,r->CustomInt7,r->CustomInt8,r->CustomInt9,r->CustomInt10,
								r->CustomStr1,r->CustomStr2,r->CustomStr3,r->CustomStr4,r->CustomStr5,
								r->CustomStr6,r->CustomStr7,r->CustomStr8,r->CustomStr9,r->CustomStr10,
								r->Datetime,r->Description,r->EventCategory,r->EventId,r->EventType,r->LogName,
								r->Recordnum,r->SourceName,r->UserName);
							sw->Close();
							sw=nullptr;
						}catch(Exception ^we)
						{}
						finally {
							if (sw != nullptr ){
								try {
									sw->Close();
									sw=nullptr;
								}catch(Exception^ ce) {}
							}
						}
					}
					//if (lastPosition % 5000 == 0) {
					for(int i=0;i < 20;i++) {
						printf("\b");
					}
					printf("%020d",lastPosition);
					//}
				} else {
					SetSendData(r);
				}
				lastPosition++;
				lastProcessed++;
				if (SetLastParams(lastPosition,r->Datetime)) {
					if (++totalProcessed < maxRecord) {
						return OPSEC_SESSION_OK;
					}
				} else {
					if (cpr != nullptr) {
						log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_record: SetLastParams failed");
					}
				}
			}catch(Exception^ ex) {
				if (cpr != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_record: Error while processing record: "+ex->Message+"\n"+ex->StackTrace+"\n");
				}
			}
			return OPSEC_SESSION_ERR;
		}

		int GetFileInfo(OpsecSession * psession,Parser::CheckPointRecorder^ cpr,long fileIdQuery,bool getNext,[Out] String^% match)
		{
			try {
				int r;

				char *pszFilename;
				int fileId;
				int accountFileId;;

				bool check=true;
				match=nullptr;
				log->Log(LogType::FILE, LogLevel::INFORM, "GetFileInfo: get first file info");
				if ((r=lea_get_first_file_info(psession,&pszFilename,&fileId,&accountFileId)) != LEA_SESSION_IT_END) {
					do {
						if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
							return 1;
						}

						if (check) {
							log->Log(LogType::FILE, LogLevel::DEBUG, "GetFileInfo: check true so check fileId. Compare for:"+fileId+"-"+fileIdQuery);
							if (fileId == fileIdQuery) {
								//CheckPointRecorder::BreakPoint("find match: get next?:"+getNext);
								check=false;
								if (getNext) {
									log->Log(LogType::FILE, LogLevel::DEBUG, "GetFileInfo: since getnext continue");
									continue;
								}
							} else {
								continue;
							}
						}
						match=fileId+","+Marshal::PtrToStringAnsi((IntPtr)pszFilename);
						log->Log(LogType::FILE, LogLevel::INFORM, "GetFileInfo: match found =>"+match);
						return 0;
					} while((r=lea_get_next_file_info(psession,&pszFilename,&fileId,&accountFileId)) != LEA_SESSION_IT_END);
					log->Log(LogType::FILE, LogLevel::INFORM, "GetFileInfo: no info found,loop terminated ");
				}
			}catch(Exception^ ex) {
				if (cpr != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "Exception in GetFileInfo: "+ex->Message+"\n"+ex->StackTrace+"\n");
				}
			}
			return 1;
		}

		int GetCollectedFileInfo(OpsecSession * psession,Parser::CheckPointRecorder^ cpr,long fileIdQuery,bool getNext,[Out] String^% match)
		{
			try {
				int r;

				int accountFileId;;

				bool check=true;
				match=nullptr;
				log->Log(LogType::FILE, LogLevel::INFORM, "GetCollectedFileInfo: get first collected file info");

				collected_file_info_t *info;
				lea_value_ex_t *fileid_ex,*filename_ex;
				long fileId;
				int actual_filenameLen;
				char filename_buff[8192];

				if ((r=lea_get_first_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL) 
				{
					do {
						if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
							log->Log(LogType::FILE, LogLevel::ERROR, "GetCollectedFileInfo: first level session error");
							return 1;
						}

						if ((fileid_ex=lea_get_collected_file_info_by_field_name(info,"FileId")) == NULL
							|| lea_value_ex_get(fileid_ex,&fileId) != OPSEC_SESSION_OK) {
								log->Log(LogType::FILE, LogLevel::ERROR, "GetCollectedFileInfo: getting file id failed");
								return 1;
						}

						if (check) {
							log->Log(LogType::FILE, LogLevel::INFORM, "GetCollectedFileInfo: check true so check fileId. Compare for:"+fileId+"-"+fileIdQuery);
							if (fileId == fileIdQuery) {
								//CheckPointRecorder::BreakPoint("find match: get next?:"+getNext);
								check=false;
								if (getNext) {
									log->Log(LogType::FILE, LogLevel::INFORM, "GetCollectedFileInfo: since getnext continue");
									continue;
								}
							} else {
								continue;
							}
						}
						if ((filename_ex=lea_get_collected_file_info_by_field_name(info,"Log Filename")) == NULL
							|| lea_value_ex_get(filename_ex,filename_buff,8192,&actual_filenameLen) != OPSEC_SESSION_OK) {
								log->Log(LogType::FILE, LogLevel::ERROR, "GetCollectedFileInfo: getting filename failed");
								return 1;
						}

						match=fileId+","+Marshal::PtrToStringAnsi((IntPtr)filename_buff);
						log->Log(LogType::FILE, LogLevel::INFORM, "GetCollectedFileInfo: match found =>"+match);
						return 0;
					} while((r=lea_get_next_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL);
					log->Log(LogType::FILE, LogLevel::INFORM, "GetCollectedFileInfo: no info found,loop terminated ");
				}
			}catch(Exception^ ex) {
				if (cpr != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "Exception in GetCollectedFileInfo: "+ex->Message+"\n"+ex->StackTrace+"\n");
				}
			}
			return 1;
		}


		int SessionFileMatchPrint(OpsecSession *psession,Parser::CheckPointRecorder^ cpr) 
		{
			try {

				int r;
				char *pszFilename;
				int normalFID;
				int accountFID;;

				System::Console::WriteLine("{0,15}{1,15}{2,40}","File Id","Account File ID","File Name");
				if ((r=lea_get_first_file_info(psession,&pszFilename,&normalFID,&accountFID)) != LEA_SESSION_IT_END) 
				{
					do 
					{
						if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
							System::Console::WriteLine("Error occured while getting file info :"+Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno))+" ("+opsec_errno+")");
							return 1;
						}
						System::Console::WriteLine("{0,15}{1,15}{2,40}",normalFID,accountFID,
							Marshal::PtrToStringAnsi((IntPtr)pszFilename));
					} while((r=lea_get_next_file_info(psession,&pszFilename,&normalFID,&accountFID)) != LEA_SESSION_IT_END);
					if ( r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
						System::Console::WriteLine("Error occured while getting file info: "+Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno))+" ("+opsec_errno+")");
					}
				}else {
					System::Console::WriteLine("!!!! Error occured while getting first file info");
				}
			}catch(Exception^ ex) {
				System::Console::WriteLine("!!!! Error occured during operations:"+ex->Message+"\n"+ex->StackTrace);
			}
			return 1;
		}

		int SessionFileMatchPrintCollected(OpsecSession *psession,Parser::CheckPointRecorder^ cpr) 
		{
			try {

				int r;
				collected_file_info_t *info;
				System::Console::WriteLine("{0,15}{1,15}{2,40}{3,15}","File Id","Creation Time","File Name","# of Records");
				if ((r=lea_get_first_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL) 
				{
					do 
					{
						if (r == LEA_SESSION_ERR || r == LEA_SESSION_NOT_AVAILABLE) {
							return 1;
						}

						lea_value_ex_t *filename_ex,*fileid_ex,*creationTime_ex,*numOfRecs_ex;
						long creationTime,fileId;

						if ((creationTime_ex=lea_get_collected_file_info_by_field_name(info,"Creation Time")) != NULL
							&& lea_value_ex_get(creationTime_ex,&creationTime) == OPSEC_SESSION_OK)
						{
							if ((fileid_ex=lea_get_collected_file_info_by_field_name(info,"FileId")) != NULL
								&& lea_value_ex_get(fileid_ex,&fileId) == OPSEC_SESSION_OK)
							{
								int numOfRecs=0;
								if ((numOfRecs_ex=lea_get_collected_file_info_by_field_name(info,"Num of Records")) != NULL
									&& lea_value_ex_get(numOfRecs_ex,&numOfRecs) == OPSEC_SESSION_OK)
								{
									int actual_filenameLen;
									char filename_buff[1024];
									if ((filename_ex=lea_get_collected_file_info_by_field_name(info,"Log Filename")) != NULL
										&& lea_value_ex_get(filename_ex,filename_buff,1024,&actual_filenameLen) == OPSEC_SESSION_OK)
									{
										System::Console::WriteLine("{0,15}{1,15}{2,40}{3,15}",fileId,creationTime,
											Marshal::PtrToStringAnsi((IntPtr)filename_buff),numOfRecs);
									} else {
										System::Console::WriteLine("!!!! Error occured while getting filename");
									}
								} else {
									System::Console::WriteLine("!!!! Error occured while getting num of records");
								}
							} else {
								System::Console::WriteLine("!!!! Error occured while getting file id");
							}
						} else {
							System::Console::WriteLine("!!!! Error occured while getting creation time");
						}
					} while((r=lea_get_next_collected_file_info(psession,&info)) != LEA_SESSION_IT_END && info != NULL);
					if (info != NULL) {
						System::Console::WriteLine("Error occured while getting file info");
					}
				}else {
					System::Console::WriteLine("!!!! Error occured while getting first file info");
				}
			}catch(Exception^ ex) {
				System::Console::WriteLine("!!!! Error occured during operations:"+ex->Message+"\n"+ex->StackTrace);
			}
			return 1;
		}

		int read_fw1_logfile_eof(OpsecSession * pSession)
		{
			Parser::CheckPointRecorder^ cpr=nullptr;
			try {
				cpr = Parser::CheckPointRecorder::getInstance();
				log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof Begins");
				fileCheckComplete=false;

				if (isolatedRegistry != nullptr) {
					for(int i=0;i < 20;i++) {
						printf("\b");
					}
					printf("%020d\n",lastPosition);
				}
				eof=true;
				char *filename_last;
				int fileid_last=0,accid_last=0;

				if (!RefreshUseCollectedFiles()) {
					useCollectedFiles=0;
					log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: RefreshUseCollectedFiles failed. So do default, don't use collected files");
				}
				if (useCollectedFiles == 0) {
					if (lea_get_last_file_info(pSession,&filename_last,&fileid_last,&accid_last) != LEA_SESSION_OK)
					{
						log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: get last file failed");
						return OPSEC_SESSION_ERR;
					}
				} else {
					collected_file_info_t *info;
					lea_value_ex_t *fileid_ex,*filename_ex;
					int actual_filenameLen;
					char filename_buff[8192];

					if (lea_get_last_collected_file_info(pSession,&info) == LEA_SESSION_IT_END || info == NULL)
					{
						log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: get last collected file failed");
						return OPSEC_SESSION_ERR;
					}

					if ((fileid_ex=lea_get_collected_file_info_by_field_name(info,"FileId")) == NULL
						|| lea_value_ex_get(fileid_ex,&fileid_last) != OPSEC_SESSION_OK) {
							log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: getting collected file id failed");
							return OPSEC_SESSION_ERR;
					}
				}

				log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: get last file info ok: Lastfile("+fileid_last+"),FileId("+fileId+")");
				//CheckPointRecorder::BreakPoint("Got lastfile="+fileid_last);
				if (fileid_last == fileId) {
					log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: last file id eof so return");
					//CheckPointRecorder::BreakPoint("This was last file so return");
					totalPass=maxLogFiles+1;
					return OPSEC_SESSION_ERR;
				}

				log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: This is not last file so go on:"+fileId);
				if (lastProcessed > 0) {
					//if at least one record processed increment
					//total pass to avoid exhausting totalPass
					//count for empty files
					++totalPass;
				}
				log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: get logfile desc");
				// Get next file
				lea_logdesc * curr=lea_get_logfile_desc(pSession);
				if (curr != NULL) 
				{
					String^ nextFilename=nullptr;

					log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: Getting next file info"+curr->fileid+"-"+fileId);
					//CheckPointRecorder::BreakPoint("Getting next file "+curr->fileid+" "+fileId+" "+nextFilename);
					if ((useCollectedFiles ==0 && !GetFileInfo(pSession,cpr,curr->fileid,curr->fileid == fileId,nextFilename) || 
						useCollectedFiles !=0 && !GetCollectedFileInfo(pSession,cpr,curr->fileid,curr->fileid == fileId,nextFilename))
						&& nextFilename != nullptr 
						&& nextFilename->Length > 0) 
					{
						log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: GetNextFile Ok, so will go on with next");
						//CheckPointRecorder::BreakPoint("Setfilename "+nextFilename+",1");
						//SetRegistry with new filename info and mark it for set lastposition=0 nexttime
						if (SetFilename(nextFilename+",1"))
						{
							//CheckPointRecorder::BreakPoint("ResetLogFilenameWith "+nextFilename+",1");
							//Now reset current file info with the same value in registry
							if (ResetLogFilenameWith(nextFilename+",1"))
							{
								//Now it's time to reset filename again to inform no need to
								//reset registry last position to zero since it's done just
								//above
								//CheckPointRecorder::BreakPoint("SetFilename again "+nextFilename+",0");
								if (SetFilename(nextFilename+",0"))
								{
									if (isolatedRegistry != nullptr) {
										if (RefreshStopOnFileId()) {
											if (fileId != stopOnFileId) {
												//CheckPointRecorder::BreakPoint("RestartAgain ");
												eof=false;
												lastProcessed=0;
												if (isolatedRegistry != nullptr) {
													Console::WriteLine("\nBegin File: Filename({0}) FileId({1}))",filename,fileId);
													printf("%020d",0);
												}
												return OPSEC_SESSION_OK;
											} else {
												log->Log(LogType::FILE, LogLevel::INFORM, "read_fw1_logfile_eof: StopOnFileId reached. Terminate operation");
												//CheckPointRecorder::BreakPoint("Stop case");
											}
										} else {
											log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: RefreshStopOnFileId failed");
											//CheckPointRecorder::BreakPoint("Refresh stop on failed");
										}
									} else {
										eof=false;
										lastProcessed=0;
										return OPSEC_SESSION_OK;
									}
								}
							}
						}
					} else {
						log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: Getting next file failed");
					}
				} else {
					//CheckPointRecorder::BreakPoint("Last file null");
					log->Log(LogType::FILE, LogLevel::ERROR, "read_fw1_logfile_eof: getting last file info failed: null");
				}
			}catch(Exception^ ex) {
				if (cpr != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "Exception in read_fw1_logfile_eof: "+ex->Message+"\n"+ex->StackTrace+"\n");
				}
			}
			return OPSEC_SESSION_ERR;
		}

		int read_fw1_logfile_session_end (OpsecSession * psession)
		{
			//Parser::CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_session_end =================");		
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			int reason=opsec_session_end_reason(psession);
			char *msg=nullptr;
			int err;
			if (!opsec_get_sic_error(psession,&err,&msg))
			{
				log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session SESSION END handler was invoked: Code("+err+")="+Marshal::PtrToStringAnsi((IntPtr)msg));
			} else {
				log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session SESSION END handler was invoked(SIC ERROR but error reason could not retrieved): "+reason);
			}
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_suspend (OpsecSession * psession)
		{
			//Parser::CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_suspend =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session SUSPEND handler was invoked: "+opsec_session_end_reason(psession));
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_resume (OpsecSession * psession)
		{
			//Parser::CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_resume =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session RESUME handler was invoked: "+opsec_session_end_reason(psession));
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_queryack (OpsecSession * psession)
		{
			//Parser::CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_queryack =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session QUERY ACK handler was invoked: "+opsec_session_end_reason(psession));
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_dict (OpsecSession * psession)
		{
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session DICT handler was invoked");
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_collogs (OpsecSession * psession)
		{
			Parser::CheckPointRecorder^ cpr=nullptr;
			try {
				cpr = Parser::CheckPointRecorder::getInstance();
				log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session COLLOGS handler was invoked");
				log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC session COLLOGS check just print");
				if (justPrint == 1) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC session COLLOGS: justPrint 1");
					SessionFileMatchPrint(psession,cpr);
				} else if (justPrint == 2) {
					log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC session COLLOGS: justPrint 2");
					SessionFileMatchPrintCollected(psession,cpr);
				} else {
					log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC session COLLOGS: session match");
					if (!SessionFileMatch(psession,cpr,false))
						return OPSEC_SESSION_OK;
				}
			}catch(Exception^ ex) {
				if (cpr != nullptr) {
					log->Log(LogType::FILE, LogLevel::ERROR, "OPSEC COLLOGS exception:"+ex->Message+"\n"+ex->StackTrace);
				}
			}
			return OPSEC_SESSION_ERR;
		}

		int read_fw1_logfile_session_start (OpsecSession * psession)
		{
			//CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_session_start =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			fileCheckComplete=false;
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session START handler was invoked");
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_established (OpsecSession * psession)
		{
			//CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_established =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC session ESTABLISHED handler was invoked");
			return OPSEC_SESSION_OK;
		}

		int read_fw1_logfile_failedconn (OpsecEntity * entity, long peer_ip, int sic_errno, char *sic_errmsg)
		{
			//CheckPointRecorder::BreakPoint("=============== read_fw1_logfile_failedconn =================");
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "OPSEC failed connection handler was invoked");
			return OPSEC_SESSION_OK;
		}

		int exit_loggrabber(int errcode)
		{
			Parser::CheckPointRecorder^ cpr = Parser::CheckPointRecorder::getInstance();
			log->Log(LogType::FILE, LogLevel::INFORM, "function cleanup_fw1_environment");

			int aa=0;
			if (pClient) {
				try {
					opsec_stop_keep_alive(pSession);
				}catch(Exception^ se) {
				}

				try {
					opsec_destroy_entity (pClient);
				} catch(Exception^ ce) {
				}
				pClient=NULL;
			}
			if (pServer) {
				try {
					opsec_destroy_entity (pServer);
				}catch(Exception^ se) {}
				pServer=NULL;
			}
			if (pEnv) {
				try {
					opsec_env_destroy (pEnv);
				}catch(Exception^ ee) {}
				pEnv=NULL;
			}

			return errcode;
		}

		int validateConfig(OpsecEnv *pEnv,Parser::CheckPointRecorder^ cpr)
		{
			char *auth_type;
			char *fw1_server;
			char *fw1_port;
			char *opsec_certificate;
			char *opsec_client_dn;
			char *opsec_server_dn;

			int xx=0;
			fw1_server = opsec_get_conf (pEnv, "lea_server", "ip", NULL);
			if (fw1_server == NULL)
			{
				log->Log(LogType::FILE, LogLevel::ERROR, "The fw1 server ip address has not been set.");
				return 1;
			}

			auth_type = opsec_get_conf (pEnv, "lea_server", "auth_type", NULL);
			if (auth_type != NULL)
			{
				//Authentication mode
				if (fw1_2000)
				{
					//V4.1.2
					fw1_port = opsec_get_conf (pEnv, "lea_server", "auth_port", NULL);
					if (fw1_port == NULL)
					{
						log->Log(LogType::FILE, LogLevel::ERROR, "The parameters about authentication mode have not been set.");
						return 1;
					}
					else
					{
						log->Log(LogType::FILE, LogLevel::DEBUG, "Authentication mode has been used.");
						log->Log(LogType::FILE, LogLevel::DEBUG, "Server-IP          :" + Marshal::PtrToStringAnsi((IntPtr)fw1_server));
						log->Log(LogType::FILE, LogLevel::DEBUG, "Server-Port        :" + Marshal::PtrToStringAnsi((IntPtr)fw1_port));
						log->Log(LogType::FILE, LogLevel::DEBUG, "Authentication type:" + Marshal::PtrToStringAnsi((IntPtr)auth_type));
					} //end of inner if
				}
				else
				{
					//NG
					fw1_port = opsec_get_conf (pEnv, "lea_server", "auth_port", NULL);
					opsec_certificate = opsec_get_conf (pEnv, "opsec_sslca_file", NULL);
					opsec_client_dn = opsec_get_conf (pEnv, "opsec_sic_name", NULL);
					opsec_server_dn = opsec_get_conf (pEnv, "lea_server", "opsec_entity_sic_name", NULL);
					if ((fw1_port == NULL) || (opsec_certificate == NULL)
						|| (opsec_client_dn == NULL)
						|| (opsec_server_dn == NULL))
					{
						log->Log(LogType::FILE, LogLevel::ERROR, "The parameters about authentication mode have not been set.");
						return 1;
					}
					log->Log(LogType::FILE, LogLevel::DEBUG, "Authentication mode has been used.");
					log->Log(LogType::FILE, LogLevel::DEBUG, "Server-IP                       :" + Marshal::PtrToStringAnsi((IntPtr)fw1_server));
					log->Log(LogType::FILE, LogLevel::DEBUG, "Server-Port                     :" + Marshal::PtrToStringAnsi((IntPtr)fw1_port));
					log->Log(LogType::FILE, LogLevel::DEBUG, "Authentication type             :" + Marshal::PtrToStringAnsi((IntPtr)auth_type));
					log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC sic certificate file name :" + Marshal::PtrToStringAnsi((IntPtr)opsec_certificate));
					log->Log(LogType::FILE, LogLevel::DEBUG, "Server DN (sic name)            :" + Marshal::PtrToStringAnsi((IntPtr)opsec_server_dn));
					log->Log(LogType::FILE, LogLevel::DEBUG, "OPSEC LEA client DN (sic name)  :" + Marshal::PtrToStringAnsi((IntPtr)opsec_client_dn));
				}
			}
			else
			{
				//Clear Text mode, i.e. non-auth mode
				fw1_port = opsec_get_conf (pEnv, "lea_server", "auth_port", NULL);
				if (fw1_port != NULL)
				{
					log->Log(LogType::FILE, LogLevel::INFORM, "Clear text mode has been used.");
					log->Log(LogType::FILE, LogLevel::INFORM, "Server-IP   :" + Marshal::PtrToStringAnsi((IntPtr)fw1_server));
					log->Log(LogType::FILE, LogLevel::INFORM, "Server-Port :" + Marshal::PtrToStringAnsi((IntPtr)fw1_port));
				}
				else
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "The fw1 server lea service port has not been set.");
					return 1;
				} //end of inner if
			} //end of middle if
			return 0;
		}

		void printRemoteSdkVersion(OpsecSession *pSession,int sdk_version,int patch_num,int build_num,char *ver_desc,char *full_desc)
		{
			Console::WriteLine("Remote SDK Ver: {0}, Patch: {1}, Build: {2}, Version: {3}, Full: {4}",sdk_version,patch_num,build_num,
				Marshal::PtrToStringAnsi((IntPtr)ver_desc),Marshal::PtrToStringAnsi((IntPtr)full_desc));
		}

		int run(int length, char **args)
		{
			Parser::CheckPointRecorder^ cpr=nullptr;
			try {
				cpr= Parser::CheckPointRecorder::getInstance();
				log->Log(LogType::FILE, LogLevel::INFORM, "Run Begins: File["+parametricFilename+"], Pos["+lastPosition+"]");
				opsec_set_debug_level(opsec_debug_level);
				/* create opsec environment for the main loop */
				if ((pEnv = opsec_init (OPSEC_CONF_ARGV, &length, args, OPSEC_EOL)) == NULL)
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "unable to create environment (" + Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno)) + ") ("+opsec_errno+")");
					return 1;
				}
				/*if (validateConfig(pEnv,cpr))
				{
					return exit_loggrabber(0);
				}*/
				log->Log(LogType::FILE, LogLevel::DEBUG,"init lea_server entity");
				pServer = opsec_init_entity (pEnv, LEA_SERVER, OPSEC_ENTITY_NAME, "lea_server", OPSEC_EOL);
				if (!pServer) {
					log->Log(LogType::FILE, LogLevel::ERROR, "failed to initialize lea server entity (" + Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno)) + ") ("+opsec_errno+")");
					return exit_loggrabber(0);
				}
				log->Log(LogType::FILE, LogLevel::DEBUG,"init lea_client_entity");
				fileCheckComplete=false;
				read_fw1_logfile_record_del=gcnew read_fw1_logfile_record_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_record);
				read_fw1_logfile_eof_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_eof);
				read_fw1_logfile_collogs_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_collogs);
				read_fw1_logfile_suspend_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_suspend);
				read_fw1_logfile_resume_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_resume);
				read_fw1_logfile_session_start_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_session_start);
				read_fw1_logfile_session_end_del=gcnew opsec_callback_delegate(this,&Parser::CheckPointRecorder::read_fw1_logfile_session_end);
				read_fw1_logfile_established_del=gcnew opsec_callback_delegate(this, &Parser::CheckPointRecorder::read_fw1_logfile_established);

				pClient = opsec_init_entity (pEnv, LEA_CLIENT,
					LEA_RECORD_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_record_del),
					LEA_EOF_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_eof_del),
					LEA_COL_LOGS_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_collogs_del),
					LEA_SUSPEND_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_suspend_del),
					LEA_RESUME_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_resume_del),
					OPSEC_SESSION_START_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_session_start_del),
					OPSEC_SESSION_END_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_session_end_del),
					OPSEC_SESSION_ESTABLISHED_HANDLER,
					Marshal::GetFunctionPointerForDelegate(read_fw1_logfile_established_del), OPSEC_EOL);
				if (!pClient)
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "failed to initialize lea client entity (" + Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno)) + ") ("+opsec_errno+")");
					return exit_loggrabber(0);
				}


				/*
				CheckPointRecorder::BreakPoint("Running with eof=false lastProc=0 recoveryMode="+recoveryMode
				+" fileId="+fileId
				+" creationTime="+creationTime
				+" filename="+filename
				+" online="+online_mode
				+" lastPos="+lastPosition);
				*/
				eof=false;
				lastProcessed=0;
				if (recoveryMode == 0 && fileId > 0
					&& filename != nullptr && filename->Length > 0) {
						if (isolatedRegistry != nullptr ) {
							if (RefreshStopOnFileId()) {
								if (fileId == stopOnFileId) {
									CheckPointRecorder::BreakPoint("Last file id reached. Press any key to terminate...");
									exit(0);
								}
							} else {
								log->Log(LogType::FILE, LogLevel::ERROR, "run: RefreshStopOnFileId failed");
								return exit_loggrabber(0);
							}
						}
						//CheckPointRecorder::BreakPoint("StartNormal");
						recoveryMode=false;
						pSession = lea_new_session (pClient, pServer,
							online_mode ? LEA_ONLINE : LEA_OFFLINE,
							fw1_2000 ? LEA_NORMAL_FILEID : LEA_UNIFIED_FILEID, fileId,
							LEA_AT_POS, lastPosition);
				} else {
					//CheckPointRecorder::BreakPoint("STart in recovery");
					recoveryMode=1;
					pSession = lea_new_session (pClient, pServer, 
						online_mode ? LEA_ONLINE : LEA_OFFLINE,
						fw1_2000 ? LEA_FIRST_NORMAL_FILEID : LEA_FIRST_UNIFIED_FILEID,
						LEA_AT_START);
				}

				if (!pSession)
				{
					log->Log(LogType::FILE, LogLevel::ERROR, "failed to create session (" + Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno)) + ") ("+opsec_errno+")");
					return exit_loggrabber(0);
				}

				int sdk_version, patch_num, build_num;
				char *ver_desc, *full_desc;

				if (isolatedRegistry != nullptr) {
					ver_desc=nullptr;
					full_desc=nullptr;
					opsec_get_sdk_version(&sdk_version,&patch_num,&build_num,&ver_desc,&full_desc);

					Console::WriteLine("Local SDK Ver: {0}, Patch: {1}, Build: {2}, Version: {3}, Full: {4}",sdk_version,patch_num,build_num,
						Marshal::PtrToStringAnsi((IntPtr)ver_desc),Marshal::PtrToStringAnsi((IntPtr)full_desc));

					printVersionDel=gcnew printRemoteSdkVersionDelegate(
						this,&Parser::CheckPointRecorder::printRemoteSdkVersion);
					if (opsec_get_peer_sdk_version(pSession,(OpsecSdkFunction)Marshal::GetFunctionPointerForDelegate(printVersionDel).ToPointer()) != -1) {
						Console::WriteLine("Peer SDK call succeed");
					} else {
						Console::WriteLine("Peer SDK call failed: "+Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno))+" ("+opsec_errno+")");
					}
				}
				if (isolatedRegistry != nullptr) {
					Console::WriteLine("Begin File: Filename({0}) FileId({1}))",filename,fileId);
				}

				opsec_start_keep_alive(pSession,15000);

				opsec_set_session_timeout(pSession,RefreshTimeout());
				if (!opsec_mainloop (pEnv)) {
					/*
					CheckPointRecorder::BreakPoint("MAIN ENDS eof=false lastProc=0 recoveryMode="+recoveryMode
					+" fileId="+fileId
					+" creationTime="+creationTime
					+" filename="+filename
					+" online="+online_mode
					+" lastPos="+lastPosition
					+" totalPass="+totalPass
					+" maxLogFiles="+maxLogFiles
					+" totalProcessed="+totalProcessed
					+" maxRecord="+maxRecord);
					*/
					if (recoveryMode > 0) {
						if (recoveryMode == 255) {
							recoveryMode=0;
							if (fileId > 0
								&& filename != nullptr && filename->Length > 0) {
									//Last recovery command handled successfully then restart soon
									//by StartHandler
									return exit_loggrabber(0);
							} else {
								recoveryMode=1;
							}
						}
						++totalPass;
					} else if (eof) {
						//File processed successfully, and set to next if available
						if (totalPass < maxLogFiles && totalProcessed < maxRecord) {
							return exit_loggrabber(0);
						}
					} else if (lastProcessed == 0) {
						//An error occured. Either by environment(like communication)
						//or file has been removed and not found. So mark it for recovery.
						//Next time it will be analyzed
						recoveryMode=1;
						return exit_loggrabber(0);
					}// else an error occured and will exit with 1
				} else {
					log->Log(LogType::FILE, LogLevel::WARN, "MAIN EXIT (" + Marshal::PtrToStringAnsi((IntPtr)opsec_errno_str (opsec_errno)) + ") ("+opsec_errno+")");
				}
			}
			catch(Exception^ re) {
				try {
					if (cpr != nullptr) {
						log->Log(LogType::FILE, LogLevel::ERROR, "Exception during run: "+re->Message+"\n" + re->StackTrace);
					}
				}catch(Exception^ le) {}
			}
			return exit_loggrabber (0);
		}
	};

}