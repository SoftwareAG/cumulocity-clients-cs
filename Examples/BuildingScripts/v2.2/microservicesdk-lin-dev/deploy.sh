#!/bin/bash

for i in "$@"; do
    case $1 in
        -url|--url) URL="$2"; shift ;;
        -u|--username) USERNAME="$2"; shift ;;
        -p|--password) PASSWORD="$2"; shift ;;
        -an|--appname) APPNAME="$2"; shift ;; # Added parameter
        -f|--file) FILE="$2"; shift ;;
    esac
    shift
done

function jsonGet {
  python -c 'import json,sys
o=json.load(sys.stdin)
k="'$1'"
if k != "":
  for a in k.split("."):
    if isinstance(o, dict):
      o=o[a] if a in o else ""
    elif isinstance(o, list):
      if a == "length":
        o=str(len(o))
      elif a == "join":
        o=",".join(o)
      else:
        o=o[int(a)]
    else:
      o=""
if isinstance(o, str) or isinstance(o, unicode):
  print o
else:
  print json.dumps(o)
'
}

cfg.parser () {
    fixed_file=$(cat $1 | sed 's/ = /=/g')  # fix ' = ' to be '='
    IFS=$'\n' && ini=( $fixed_file )              # convert to line-array
		
    ini=( ${ini[*]//;*/} )                   # remove comments
    ini=( ${ini[*]/#[/\}$'\n'cfg.section.} ) # set section prefix
    ini=( ${ini[*]/%]/ \(} )                 # convert text2function (1)
    ini=( ${ini[*]/=/=\( } )                 # convert item to array
    ini=( ${ini[*]/%/ \)} )                  # close array parenthesis
    ini=( ${ini[*]/%\( \)/\(\) \{} )         # convert text2function (2)
    ini=( ${ini[*]/%\} \)/\}} )              # remove extra parenthesis   	
	ini[0]=''                                # remove first element
    ini[${#ini[*]} + 1]='}'                  # add the last brace
	
	
    eval "$(echo "${ini[*]}")"               # eval the result
}

function geturl() {
    
	elements=$(curl --user $cred  -s http://$url/application/applicationsByName/$appname | jsonGet  applications.length)
	if !([ -z $elements ]) && ([ $elements -gt 0 ]) then
        for value in $elements
        do

        selectedapp=$(curl --user $cred  -s http://$url/application/applicationsByName/$appname | jsonGet  applications.$((value-1)).name)

        if [ "$selectedapp" == "$appname" ]; then
           appid=$(curl --user $cred  -s http://$url/application/applicationsByName/$appname | jsonGet  applications.$((value-1)).id)

          break
        fi

        done
	fi	
	
	if [ "$appid" -eq 0 ]; then
		echo "Error! The app not found" 1>&2
		exit 64
	fi	

	url="http://$url/application/applications/$appid/binaries"
}

function abspath() {
    # generate absolute path from relative path
    # $1     : relative filename
    # return : absolute path
    if [ -d "$1" ]; then
        # dir
        (cd "$1"; pwd)
    elif [ -f "$1" ]; then
        # file
        if [[ $1 = /* ]]; then
            echo "$1"
        elif [[ $1 == */* ]]; then
            echo "$(cd "${1%/*}"; pwd)/${1##*/}"
        else
            echo "$(pwd)/$1"
        fi
    fi
}

credb64=""
header="Basic "
url="http://URL/application/applications/0/binaries"
cred="username:password"
dir="/images/multi/image.zip"



if [ -z "$URL" ] && [ -z "$USERNAME" ] && [ -z "$PASSWORD" ] && [ -z "$APPNAME" ] && [ -z "$FILE" ];
then
    echo "All is null."
	
	filePath=$(abspath settings.ini)
	
	if [ -z $filePath ]; then
         pathHomeSettings="$HOME/.c8y/settings.ini"
         filePath=$(abspath $pathHomeSettings)
         if [ -z $filePath ]; then
			echo "The file not found." 1>&2
			exit 64
         fi
	else
			 echo $filePath
	fi
	

	cfg.parser $filePath
	cfg.section.deploy

	echo "$url"
	echo "$username"
	echo "$appname"
	
	cred="$username:$password"	
	appid=0
	
	geturl
	
elif !([ -z "$FILE" ]) && ([ -z "$URL" ] && [ -z "$USERNAME" ] && [ -z "$PASSWORD" ] && [ -z "$APPNAME" ]); then
    
	echo "File not null, other is null."
	
	filePath=$(abspath $FILE)
	
	if [ -z $filePath ]; then
         pathHomeSettings="$HOME/.c8y/$FILE"
         filePath=$(abspath $pathHomeSettings)
         if [ -z $filePath ]; then
			echo "The file not found." 1>&2
			exit 64
         fi
	else
			 echo $filePath
	fi
	
	
	
	cfg.parser $filePath
	cfg.section.deploy

	echo "$url"
	echo "$username"
	echo "$appname"
	
	cred="$username:$password"
	
	appid=0
	
	geturl
	
elif !([ -z "$FILE" ]) && ( !([ -z "$URL" ]) || !([ -z "$USERNAME" ]) || !([ -z "$PASSWORD" ]) || !([ -z "$APPNAME" ])  ); then
    
	filePath=$(abspath $FILE)
	
	if [ -z $filePath ]; then
         pathHomeSettings="$HOME/.c8y/$FILE"
         filePath=$(abspath $pathHomeSettings)
         if [ -z $filePath ]; then
			echo "The file not found." 1>&2
			exit 64
         fi
	else
			 echo $filePath
	fi
	
	cfg.parser $filePath
	cfg.section.deploy

	
	if  !([ -z "$URL" ]);
	then
	   url=$URL
	fi
	
	if  !([ -z "$USERNAME" ]);
	then
	   username=$USERNAME
	fi
	
	if  !([ -z "$PASSWORD" ]);
	then
	   password=$PASSWORD
	fi
	
	if  !([ -z "$APPNAME" ]);
	then
	   appname=$APPNAME
	fi
	
	echo $username
	echo "$username"
	echo "$appname"
	
	cred="$username:$password"	
	appid=0
	
	geturl
	
else 
   echo "..."
fi


credb64=`echo -n "$cred" | base64`
header=`echo "$header$credb64="`

echo "sending file..."
echo $credb64
echo $header

echo $(curl -v -X POST  \
			-H "Accept: application/json" \
			-H "authorization: $header" \
			-H 'content-type: application/x-www-form-urlencoded' \
			--data-binary "@$dir" \
			"$url" )
