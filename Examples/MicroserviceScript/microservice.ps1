$global:WORK_DIR=$(pwd)
$global:IMAGE_NAME=""
$global:TAG_NAME="latest"
$global:DEPLOY_ADDRESS=""
$global:DEPLOY_TENANT=""
$global:DEPLOY_USER=""
$global:DEPLOY_PASSWORD=""
$global:APPLICATION_NAME=""
$global:APPLICATION_ID=""

$global:PACK=1
$global:DEPLOY=1
$global:SUBSCRIBE=1
$global:HELP=1


function execute
{
   [CmdletBinding()]
    Param(
        [Object[]]$args
    )


    readInput $args
    
    cd $WORK_DIR
    if("$HELP".equals("0"))
    {
       printHelp;
       exit;
    }

	if( "$PACK".equals("1") -and  "$DEPLOY".equals("1") -and  "$SUBSCRIBE".equals("1"))
	{
		Write-Output "[INFO] No goal set. Please set pack, deploy or subscribe"
	}

	if("$PACK".equals("0"))
	{
		echo "[INFO] Start packaging"
		#verifyPackPrerequisits
		#clearTarget
		#buildImage
		#exportImage
		#zipFile
		Write-Output "[INFO] End packaging"
	}
	if( "$DEPLOY".Equals("0"))
	{
		Write-Output "[INFO] Start deployment"
		#deploy
		Write-Output "[INFO] End deployment"
	}
	if("$SUBSCRIBE".Equals("0"))
	{
		Write-Output  "[INFO] Start subsciption"
		subscribe
		Write-Output  "[INFO] End subsciption"
	}
    exit; 
}


function readInput
{
   [CmdletBinding()]
    Param(
        [Object[]]$args
    )

    #write-output "count: $($args.count)"
    #write-output "passed arguments:"$args
    
    Write-Output "[INFO] Read input"

    For ($i=0; $i -le $args.Count; $i++) 
    {
        if($args[$i] -eq "pack"){
            $global:PACK=0;
        }
        if($args[$i] -eq "deploy"){
            $global:DEPLOY=0;
        }
        if($args[$i] -eq "subscribe"){
            $global:SUBSCRIBE=0;
        }
        if(($args[$i] -eq "help") -or ($args[$i] -eq "--help")){
            $global:HELP=0;
        }
        if(($args[$i] -eq "-dir") -or ($args[$i] -eq "--directory")){
            $global:WORK_DIR=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-n") -or ($args[$i] -eq "--name")){
            $global:IMAGE_NAME=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-t") -or ($args[$i] -eq "--tag")){
            $global:TAG_NAME=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-d") -or ($args[$i] -eq "--deploy")){
            $global:DEPLOY_ADDRESS=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-u") -or ($args[$i] -eq "--user")){
            $global:DEPLOY_USER=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-p") -or ($args[$i] -eq "--password")){
            $global:DEPLOY_PASSWORD=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-te") -or ($args[$i] -eq "--tenant")){
            $global:DEPLOY_TENANT=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-a") -or ($args[$i] -eq "--application")){
            $global:APPLICATION_NAME=$args[$i+1]
            $i=$i+1
        }
        if(($args[$i] -eq "-id") -or ($args[$i] -eq "--applicationId")){
            $global:APPLICATION_ID=$args[$i+1]
            $i=$i+1
        }   
    }

    setDefaults
}

function setDefaults() 
{
	$ZIP_NAME="$IMAGE_NAME.zip"
	if( "x$APPLICATION_NAME".Equals("x"))
	{
		$APPLICATION_NAME=$IMAGE_NAME
	}	
}

function printHelp() {

	Write-Output "Following functions are available. You can run specify them in single execution:"
	Write-Output "	pack - prepares deployable zip file. Requires following stucture:"
	Write-Output "		/docker/Dockerfile"
	Write-Output "		/docker/* - all files within the directory will be included in the docker build"
	Write-Output "		/cumulocity.json "
	Write-Output "	deploy - deploys applicaiton to specified address"
	Write-Output "	subscribe - subscribes tenant to specified microservice application"
	Write-Output "	help | --help - prints help"

	Write-Output "Following options are available:"
	Write-Output "	-dir | --directory 		# Working directory. Default value'$(pwd)' "
	Write-Output "	-n   | --name 	 		# Docker image name"
	Write-Output "	-t   | --tag			# Docker tag. Default value 'latest'"
	Write-Output "	-d   | --deploy			# Address of the platform the microservice will be uploaded to"	
	Write-Output "	-u   | --user			# Username used for authentication to the platform"
	Write-Output "	-p   | --password 		# Password used for authentication to the platform"
	Write-Output "	-te  | --tenant			# Tenant used"
	Write-Output "	-a   | --application 	# Name upon which the application will be registered on the platform. Default value from --name parameter"
	Write-Output "	-id  | --applicationId	# Applicaiton used for subscription purposes. Required only for solemn subscribe execution"
}

$deployResult=0
function verifyDeployPrerequisits() {

	verifyParamSet "$IMAGE_NAME" "name"
	verifyParamSet "$DEPLOY_ADDRESS" "address"
	verifyParamSet "$DEPLOY_TENANT" "tenant"
	verifyParamSet "$DEPLOY_USER" "user"
	verifyParamSet "$DEPLOY_PASSWORD" "password"
	
	if("$deployResult".Equals("1"))
	{
		Write-Output "[WARNING] Deployment skiped"
		exit 1
	}
}

function verifyParamSet()
{
    [CmdletBinding()]
    Param(
        [string]$param1,
        [string]$param2
    )

    Write-Output "x$param1"

	if("x$param1".Equals("x"))
	{
		Write-Output "[WARNING] Missing parameter: $param2"
		$deployResult=1
	}	
}

function subscribe (){
    verifySubscribePrerequisits
    
    $Credentials = "${$DEPLOY_USER}:${$DEPLOY_PASSWORD}"
	$Bytes = [System.Text.Encoding]::Unicode.GetBytes($Text)
    $authorization =[Convert]::ToBase64String($Bytes)

	
	Write-Output "[INFO] Tenant $DEPLOY_TENANT subscription to application $APPLICATION_NAME with id $APPLICATION_ID"
	#body="{\"application\":{\"id\": \"$APPLICATION_ID\"}}"
	#resp=$(curl -X POST -s -d "$body"  -H "Authorization: $authorization" -H "Content-type: application/json" "$DEPLOY_ADDRESS/tenant/tenants/$DEPLOY_TENANT/applications")
	#if [ "x$(echo $resp | jq -r .error)" != "xnull" ] && [ "x$(echo $resp | jq -r .error)" != "x" ]
	#then		
	#	echo "[WARNING] error subscribing tenant to application "
	#	echo "$(echo $resp | jq -r .message)"
	#fi
	#if [ "x$(echo $resp | jq -r .error)" == "x" ]
	#then		
	#	echo "[INFO] Tenant $DEPLOY_TENANT subscribed to application $APPLICATION_NAME"
    #fi

    try 
    {
        Invoke-RestMethod -Method Post -Uri "$DEPLOY_ADDRESS/tenant/tenants/$DEPLOY_TENANT/applications" -Header "Authorization: $authorization" -Body $body -ContentType "application/json"
    } catch {
        # Dig into the exception to get the Response details.
        # Note that value__ is not a typo.
        Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
        Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription
    }
}

function  verifySubscribePrerequisits(){
	if("x$APPLICATION_ID".Equals("x"))
	{
		Write-Output "[ERROR] Subscription not possible uknknown applicaitonId"
		exit 1
	}	
	verifyDeployPrerequisits
}

execute $args

