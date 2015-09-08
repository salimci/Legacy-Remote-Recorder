#!/bin/bash

if [[ $# != 3 ]]
then
	echo "Usage: [output file] [max file size in bytes] [sleep time]"
	exit 1
fi

while((1))
do
	for i in 1 2 3 4 5 6
	do
		echo "I: $i,Date:"$(date) >> "$1"
		usleep 500
	done

	b=$(stat --printf="%s" "$1")

	if (($b > $2))
	then

		next=$(ls -1 "$1"*|grep -E '^(.*/secure|secure)[0-9][0-9]*$'|awk -F '/' '{print $NF}'|sed 's/^.*[^0-9]\([0-9][0-9]*\)$/\1/g'|sort -rn|head -1)

		if [[ -z "$next" ]]
		then
			next=1
		else
			next=$((next+1))
		fi
		mv "$1" "$1"$next && touch "$1"
		sleep "$3"
	fi
done
