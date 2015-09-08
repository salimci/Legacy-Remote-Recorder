#!/bin/bash


>~/vars.dat

if [[ -z "$3" ]]
then
	ls -1i "$2"|grep -E "^[0-9][0-9]*[ \t][ \t]*$4$"
else
	ls -1i "$2"|grep -E "^$3[ \t][ \t]*.*$"
fi|if read _inode _file
then
	echo $_inode$'\t'$(stat -c %Y $2/$_file)$'\t'$_file|if IFS=$'\t' read INODE CTIME FILE
			then
				if [[ ! -z "$FILE" ]]
				then
					echo "$1;BEGIN;OK"
					echo "FILE;$INODE;$CTIME;$FILE"
					echo "OUTPUT;BEGIN"
					if echo "$FILE"|grep -q "\.gz$"
					then
						gunzip -c "$2/$FILE"
					else
						cat "$2/$FILE"
					fi|sed -n "$7,$8p;"|sed 's/^/+/'
				fi
			fi
else
	ls -1i "$2"|grep -E "$5"|while read _inode _file
	do
		echo $_inode $_file $(stat -c %Y $2/$_file)
	done|sort -n -k3|(found=0; while read _inode _file _ctime
	do
		if (( $_ctime > $6  ))
		then
			echo "$1;BEGIN;NEW"
			echo "FILE;$_inode;$_ctime;$_file"
			echo "$1;ENDS"
			found=1
			break;
		fi
	done
	if [[ $found == 0 ]]
	then
		echo "$1;NOFILE"
	fi)
fi

