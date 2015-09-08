/*
 * CheckPoint Recorder
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

// stdafx.cpp : source file that includes just the standard includes
// OpsecTest.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"
#include "CheckPointRecorder.h"
using namespace Parser;

int main(int argc, char *args[])
{
	String^ isolatedRegKey=nullptr;
	String^ outputFile=nullptr;
	String^ outputFile2=nullptr;
	int justPrint=0;

	if (argc >= 2) {
		isolatedRegKey=System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)args[1]);
		if (argc >= 3) {
			justPrint=!strcmpi("--print",args[2]) ? 1 : (!strcmpi("--print-collected",args[2]) ? 2 : 0);
		}
	} else {
		char line[1024];

		printf("Run in Isolated Mode?[yY/nN, default no]:");
		if (gets(line) > 0) {
			if (!strcmpi(line,"y"))
			{
				printf("Isolated Instance Registry Key: ");
				if (gets(line) > 0) {
					isolatedRegKey=System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)line);
					printf("Would you like to just print server files?[yY/nN, default no]:");
					if (gets(line) >0)
					{
						if(!strcmpi(line,"y"))
							justPrint=1;
						else if(!strcmpi(line,"yc"))
							justPrint=2;
						else if (!strnicmp(line,"log ",4))
						{
							int i=3;
							while(line[++i] && (line[i] == ' ' || line[i] == '\t'));
							if (line[i]) {
								outputFile=System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)&line[i]);
							}
							printf("Output File: [%s]\n",outputFile);
						}else if (!strnicmp(line,"log2 ",5))
						{
							int i=4;
							while(line[++i] && (line[i] == ' ' || line[i] == '\t'));
							if (line[i]) {
								outputFile2=System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)&line[i]);
							}
							printf("Output File2: [%s]\n",outputFile2);
						}
					}
				}
			}
		}
	}

	CheckPointRecorder^ cpr=gcnew CheckPointRecorder(isolatedRegKey,nullptr);
	cpr->justPrint=justPrint;
	outputFile=outputFile;
	outputFile2=outputFile2;
	cpr->Start();
	return(0);
}
