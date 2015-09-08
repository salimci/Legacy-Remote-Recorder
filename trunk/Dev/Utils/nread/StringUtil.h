/*
 * nread
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

#pragma once

class StringUtil
{
public:
	//static bool CompareW(const std::wstring& s1, const std::wstring& s2);
	static bool Compare(const std::string& s1, const std::string& s2);
	static std::string FixString(std::string& str);
	//static std::wstring FixStringW(std::wstring& str);
	static std::string Trim(std::string& str);

	static uint32 SplitString(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties);
	/*static uint32 SplitStringW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties);*/
	static uint32 SplitStringTo(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 to);
	/*static uint32 SplitStringToW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 to);*/
	static uint32 SplitStringFrom(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 from);
	/*static uint32 SplitStringFromW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 from);*/
	static uint32 SplitString(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 to, int32 from);
	/*static uint32 SplitStringW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 to, int32 from);*/
};
