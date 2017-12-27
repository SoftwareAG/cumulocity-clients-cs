#!/bin/bash

credb64=""
header="Basic "
url="http://management.staging7.c8y.io/application/applications/14/binaries"
cred="management/admin:Pyi1bo1r"
dir="/c/Users/pnow/Documents/Hg/ClientsCS/DockerWebApp.Demo/DockerWebApp.Demo/hello-world-multi/image.zip"

credb64=`echo -n "$cred" | base64`
header=`echo "$header$credb64="`

echo "sending file..."

echo $(curl -v -X POST  \
			-H "Accept: application/json" \
			-H "authorization: $header" \
			-H 'content-type: application/x-www-form-urlencoded' \
			--data-binary "@$dir" \
			"$url" )
