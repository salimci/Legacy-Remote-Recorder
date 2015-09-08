/*
 * nread
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

#include "stdafx.h"

bool StringUtil::Compare(const std::string &s1, const std::string &s2)
{
	std::string::const_iterator itr1 = s1.begin();
	std::string::const_iterator itr2 = s2.begin();
	for(; itr1 != s1.end() && itr2 != s2.end(); itr1++,itr2++)
	{
		if((*itr1) == (*itr2))
			continue;
		return false;
	}
	return true;
}

/*bool StringUtil::CompareW(const std::wstring& s1, const std::wstring& s2)
{
	std::wstring::const_iterator itr1 = s1.begin();
	std::wstring::const_iterator itr2 = s2.begin();
	for(; itr1 != s1.end() && itr2 != s2.end(); itr1++,itr2++)
	{
		if((*itr1) == (*itr2))
			continue;
		return false;
	}
	return true;
}*/

std::string StringUtil::FixString(std::string& str)
{
	std::stringstream result;
	std::string::const_iterator itr = str.begin();
	for(; itr != str.end(); itr++)
	{
		if((*itr) == 13)
			return result.str();
		else
			result << (*itr);
	}
	return result.str();
}

/*std::wstring StringUtil::FixStringW(std::wstring& str)
{
	std::wstringstream result;
	std::wstring::const_iterator itr = str.begin();
	for(; itr != str.end(); itr++)
	{
		if((*itr) == 13)
			return result.str();
		else
			result << (*itr);
	}
	return result.str();
}*/

std::string StringUtil::Trim(std::string& str)
{
	std::stringstream result;
	std::string::const_iterator itr = str.begin();
	bool endme = false;
	for(; itr != str.end(); itr++)
	{
		if((*itr) == 0)
		{
			if(endme)
				return result.str();
			endme = true;
			continue;
		}
		else
		{
			if(endme)
				endme = false;
			result << (*itr);
		}
	}
	return result.str();
}

uint32 StringUtil::SplitString(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
        positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::string s("");
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}

/*uint32 StringUtil::SplitStringW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
        positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::wstring s(L"");
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}*/

uint32 StringUtil::SplitStringTo(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 to)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
        positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::string s("");
		if(i >= to)
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  input.length() );
				}
			}
			if( includeEmpties || ( s.size() > 0 ) )
			{
				results.push_back(s);
			}
			numFound++;
			break;
		}
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}

/*uint32 StringUtil::SplitStringToW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 to)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
        positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::wstring s(L"");
		if(i >= to)
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  input.length() );
				}
			}
			if( includeEmpties || ( s.size() > 0 ) )
			{
				results.push_back(s);
			}
			numFound++;
			break;
		}
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}*/

uint32 StringUtil::SplitStringFrom(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 from)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
		if(numFound > from)
			positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::string s("");
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}

/*uint32 StringUtil::SplitStringFromW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 from)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
		if(numFound >= from)
			positions.push_back(newPos);
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
    {
        return 0;
    }

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::wstring s(L"");
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}*/

uint32 StringUtil::SplitString(const std::string& input, 
       const std::string& delimiter, std::vector<std::string>& results, 
       bool includeEmpties, int32 to, int32 from)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;
	int32 toAdd = 0;
    while( newPos >= iPos )
    {
        numFound++;
		if(numFound >= from)
			positions.push_back(newPos);
		else
			toAdd++;
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if(numFound == 0 || !positions.size())
    {
        return 0;
    }

	int32 newTo = to - toAdd;
    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::string s("");
		if(i >= newTo)
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  input.length() );
				}
			}
			if( includeEmpties || ( s.size() > 0 ) )
			{
				results.push_back(s);
			}
			numFound++;
			break;
		}
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}

/*uint32 StringUtil::SplitStringW(const std::wstring& input, 
       const std::wstring& delimiter, std::vector<std::wstring>& results, 
       bool includeEmpties, int32 to, int32 from)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( 
        ( isize == 0 )
        ||
        ( sizeS2 == 0 )
    )
    {
        return 0;
    }

    std::vector<int> positions;

    newPos = input.find (delimiter, 0);

    if( newPos < 0 )
    { 
        return 0; 
    }

    int numFound = 0;
	int32 toAdd = 0;
    while( newPos >= iPos )
    {
        numFound++;
		if(numFound >= from)
			positions.push_back(newPos);
		else
			toAdd++;
        iPos = newPos;
        newPos = input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 || !positions.size())
    {
        return 0;
    }

	int32 newTo = to - toAdd;
    for( int i=0; i <= (int)positions.size(); ++i )
    {
        std::wstring s(L"");
		if(i >= newTo)
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  input.length() );
				}
			}
			if( includeEmpties || ( s.size() > 0 ) )
			{
				results.push_back(s);
			}
			numFound++;
			break;
		}
        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;
			if( offset < isize )
			{
				if( i == positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}
        if( includeEmpties || ( s.size() > 0 ) )
        {
            results.push_back(s);
        }
    }
    return numFound;
}*/

