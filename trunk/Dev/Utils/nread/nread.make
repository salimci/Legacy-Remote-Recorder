# to build this file use command: make -f nread.make
# nread.make -- this is a comment line, ignored by make utility

nread.out : stdafx.o StringUtil.o nread.o
	g++ -static -Wall -O2 -lm -o nread stdafx.o StringUtil.o nread.o
# above, we are saying nread depends on stdafx.o, StringUtil.o and nread.o
# and to create nread we give the g++ command as shown on the next line

# which starts with a TAB although you cannot see that .
# note that the command : g++ -o nread.out stdafx.o StringUtil.o nread.o
# creates an executable file named nread from the 3 object files respectively.
stdafx.o: stdafx.cpp stdafx.h
	g++ -c stdafx.cpp
# above we are saying stdafx.o depends on main1.cpp openfile.h and mylib.h
# and to compile only stdafx.cpp if and only if stdafx.cpp or StringUtil.h or nread.h
# have changed since the last creation of main1.o
StringUtil.o: StringUtil.cpp stdafx.h
	g++ -c StringUtil.cpp
# above we are saying mylib.o depends on mylib.cpp and mylib.h
# so if either mylib.cpp or mylib.h CHANGED since creating mylib.o
# comple mylib.cpp again
nread.o: nread.cpp stdafx.h
	g++ -c nread.cpp
# above we are saying openfile.o depends on openfile.cpp and openfile.h
# so if either openfile.cpp or openfile.h has CHANGED since creating
# openfile.o, compile only (again) openfile.cpp
clean:
	rm *.o nread
# above we are stating how to run the rule for clean, no dependencies,
# what we want is when we ask to do a "make -f nread.make clean"
# that will not do anything except remove executable and object files
# so we can "clean out" our directory of unneeded large files.
# we only do a make clean when we want to clean up the files.

# END OF MAKE FILE 
