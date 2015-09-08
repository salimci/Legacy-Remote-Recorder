#pragma once
#include "stdafx.h"
#include "opsec/lea.h"
#include "opsec/lea_filter.h"
#include "opsec/lea_filter_ext.h"
#include "opsec/opsec.h"

using namespace System;

namespace Parser {
	public delegate bool Lea2RecFieldDelegate(CustomTools::CustomBase::Rec^,int,int, String^, OpsecSession *, lea_record * , int [], Object^ ctxArgs,Log::CLogger^);
	public ref class Lea2RecField
	{
	public:
		Object^ ctxArgs;
		Lea2RecFieldDelegate^ lea2RecFieldDelegate;
		Lea2RecField(void);
		Lea2RecField(Object^ ctxArgs,Lea2RecFieldDelegate^ lea2RecFieldDelegate);
	};
}
