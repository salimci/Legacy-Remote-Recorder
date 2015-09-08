#include "StdAfx.h"
#include "Lea2RecField.h"

namespace Parser {
	Lea2RecField::Lea2RecField(void)
	{
		ctxArgs=nullptr;
		lea2RecFieldDelegate=nullptr;
	}

	Lea2RecField::Lea2RecField(Object^ ctxArgs,Lea2RecFieldDelegate^ lea2RecFieldDelegate)
	{
		this->ctxArgs=ctxArgs;
		this->lea2RecFieldDelegate=lea2RecFieldDelegate;
	}
}