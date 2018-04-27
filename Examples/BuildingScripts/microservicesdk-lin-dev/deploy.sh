#!/bin/bash

for i in "$@"; do
    case $1 in
        -t|--tenant) TENANT="$2"; shift ;;
        -u|--username) USERNAME="$2"; shift ;;
        -p|--password) PASSWORD="$2"; shift ;;
        -i|--appid) APPID="$2"; shift ;; # Added parameter
        -f|--file) FILE="$2"; shift ;;
    esac
    shift
done

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
url="http://management.staging.c8y.io/application/applications/1/binaries"
cred="tenant/username:password"
dir="/images/multi/image.zip"



if [ -z "$TENANT" ] && [ -z "$USERNAME" ] && [ -z "$PASSWORD" ] && [ -z "$APPID" ] && [ -z "$FILE" ];
then
    echo "All is null."
	
	filePath=$(abspath settings.ini)
	cfg.parser $filePath
	cfg.section.deploy

	echo "$tenant"
	echo "$username"
	echo "$password"
	echo "$appid"
	
	cred="$username\$password"
	url="http://$tenant/application/applications/$appid/binaries"
	
elif !([ -z "$FILE" ]) && ([ -z "$TENANT" ] && [ -z "$USERNAME" ] && [ -z "$PASSWORD" ] && [ -z "$APPID" ]); then
    
	echo "File not null, other is null."
	
	filePath=$(abspath $FILE)
	cfg.parser $filePath
	cfg.section.deploy

	echo "$tenant"
	echo "$username"
	echo "$password"
	echo "$appid"
	
	cred="$username\$password"
	url="http://$tenant/application/applications/$appid/binaries"
	
elif !([ -z "$FILE" ]) && ( !([ -z "$TENANT" ]) || !([ -z "$USERNAME" ]) || !([ -z "$PASSWORD" ]) || !([ -z "$APPID" ])  ); then
    
	filePath=$(abspath $FILE)
	cfg.parser $filePath
	cfg.section.deploy

	echo "$tenant"
	echo "$username"
	echo "$password"
	echo "$appid"
	
	if  !([ -z "$TENANT" ]);
	then
	   $tenant=$TENANT
	fi
	
	if  !([ -z "$USERNAME" ]);
	then
	   $username=$USERNAME
	fi
	
	if  !([ -z "$PASSWORD" ]);
	then
	   $password=$PASSWORD
	fi
	
	if  !([ -z "$APPID" ]);
	then
	   $appid=$APPID
	fi
	
	cred="$username\$password"
	url="http://$tenant/application/applications/$appid/binaries"
else 
   echo "A"
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
