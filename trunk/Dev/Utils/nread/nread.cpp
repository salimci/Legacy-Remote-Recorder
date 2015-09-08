/*
 * nread
 * Copyright (C) 2009 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

#include "stdafx.h"

int main(int argc, char* argv[])
{
	if(argc < 2)
		return 0;

	std::string argswitch = argv[1];

	if(argswitch == "-n")
	{
		if(argc < 4)
			return 0;

		std::string str = argv[2];
		std::vector<std::string> results;
		StringUtil::SplitString(str, ",", results, false);
		
		uint64 left = atol(results[0].c_str());
		std::string rightS = results[1];
		if(rightS.size() < 2)
			return 0;

		rightS.erase(rightS.size() - 1, 1);
		uint64 right = atol(rightS.c_str());

		std::string file = argv[3];
		std::ifstream ifs(file.c_str());

		if(ifs)
		{
			ifs.seekg (static_cast<long>(left), std::ios::beg);
			uint64 position = left;
			uint64 lastPosition = left;
			int32 count = 0;
			std::string lastLine = "";
			while(!ifs.eof())
			{
				std::string line;
				std::getline(ifs, line);
				count++;
				if(count > right)
					break;
				if(line != "")
				{
					lastPosition = position;
					lastLine = line;
					printf("%s\n", line.c_str());
					if(!ifs.eof())
						position = static_cast<long>(ifs.tellg());
				}
			}

			std::stringstream output;
			output << "~?`Position:" << lastPosition << "\tLastline`" << lastLine << "\n";
			printf("%s", output.str().c_str());
			/*lastLine.push_back('\0');
			printf("~?`Position:%ld\tLastline:%s\n", position, lastLine.c_str());*/
			//std::cout << "~?`Position:" << position << "\tLastline:" << lastLine << "\n";
			ifs.close();
		}
	}
	else
	{
		std::string file = argv[2];
		std::ifstream ifs(file.c_str());

		if(ifs)
		{
			uint64 position = 0;
			uint64 lastPosition = 0;
			while(!ifs.eof())
			{
				std::string line;
				std::getline(ifs, line);
				if(line != "")
				{
					lastPosition = position;
					if(!ifs.eof())
						position = static_cast<long>(ifs.tellg());
				}
			}

			std::stringstream output;
			output << lastPosition << " " << file.c_str() << "\n";
			printf("%s", output.str().c_str());
			ifs.close();
		}
	}

	return 0;
}
