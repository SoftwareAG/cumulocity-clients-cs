<#
.SYNOPSIS
    Script to increment project versions for beta, minor releases and hotfixes.

.DESCRIPTION
    Cumulocity provides you with an utility tool for easy microservice packaging, deployment and subscription. The script requires running docker.
    https://docs.adamos.com/guides/reference/microservice-package/

.PARAMETER mode 
    Specify 'beta', 'release' or 'hotfix'

.EXAMPLE 
    microservice.ps1 pack deploy subscribe -n hello-world -d {url} -u {username} -p {password} -te {tenant}

#>

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
		verifyPackPrerequisits
		clearTarget
		buildImage
		exportImage
		zipFile
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

function  verifyPackPrerequisits() {
	Write-Output  "[INFO] Check input"
	result=0
	verifyParamSet "$IMAGE_NAME" "name"
	#isPresent $(find -maxdepth 1 -name "docker" | wc -l) "[ERROR] Stopped: missing docker directory in work directory: $WORK_DIR"
	#isPresent $(find docker -maxdepth 1 -name "Dockerfile" | wc -l) "[ERROR] Stopped: missing dockerfile in work directory: $WORK_DIR"
	#isPresent $(find -maxdepth 1 -name "cumulocity.json" | wc -l) "[ERROR] Stopped: missing cumulocity.json in work directory: $WORK_DIR"

	#if [ "$result" == "1" ]
	#then
	#	echo "[WARNING] Pack skiped"
	#	exit 1
	#fi
}

function isPresent() {
    Write-Output "[INFO] Check input"
    #
}

function clearTarget(){
	Write-Output "[INFO] Clear target files"
    #
}

function buildImage(){
    #
}

function  exportImage(){
    #
}

function zipFile(){
   #
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

function push(){

    $Credentials = "${$DEPLOY_USER}:${$DEPLOY_PASSWORD}"
	$Bytes = [System.Text.Encoding]::Unicode.GetBytes($Text)
    $authorization =[Convert]::ToBase64String($Bytes)
	
	$APPLICATION_ID=$(getApplicationId)

	if( "x$APPLICATION_ID".Equals("x"))
	{
		Write-Output "[INFO] Application with name $APPLICATION_NAME not found, add new application"
		createApplication $authorization
		$APPLICATION_ID=$(getApplicationId)

		if("x$APPLICATION_ID".Equals("x"))
		{
			Write-Output  "[ERROR] Could not create application"
			EXIT 1
		}
	}
	Write-Output "[INFO] Application name: $APPLICATION_NAME id: $APPLICATION_ID"
	
	uploadFile	
}

function getApplicationId(){

    $Result = Invoke-RestMethod -Uri "$DEPLOY_ADDRESS/application/applicationsByName/$APPLICATION_NAME" -Headers @{Authorization=("Basic {0}" -f $authorization)} -ErrorVariable RestError -ErrorAction "SilentlyContinue"

    if ($RestError)
    {
        $HttpStatusCode = $RestError.ErrorRecord.Exception.Response.StatusCode.value__
        $HttpStatusDescription = $RestError.ErrorRecord.Exception.Response.StatusDescription
    
        Write-Output "[ERROR] Error while connecting to platform"
        Write-Output "Http Status Code: $($HttpStatusCode) `nHttp Status Description: $($HttpStatusDescription)"
        Exit        
    }
    else
    {
        Write-Output  Result.applications.id
    }
}

function  createApplication() {
[CmdletBinding()]
    Param(
        [string]$authorization
    )

	$body="{
			""name"": ""$APPLICATION_NAME"",
			""type"": ""MICROSERVICE"",
			""key"":  ""$APPLICATION_NAME-microservice-key""
		    }";

    $Result = Invoke-RestMethod -Method Post -Uri  "$DEPLOY_ADDRESS/application/applications" -Headers @{Authorization=("Basic {0}" -f $authorization)} -ErrorVariable RestError -ErrorAction "SilentlyContinue"
}


function deploy() {
	verifyDeployPrerequisits
	push
}

function uploadFile(){

	Write-Output "[INFO] Upload file $WORK_DIR/$ZIP_NAME"

	#resp=$(curl -F "data=@$WORK_DIR/$ZIP_NAME" -H "Authorization: $authorization" "$DEPLOY_ADDRESS/application/applications/$APPLICATION_ID/binaries")
	
    #if [ "x$(echo $resp | jq -r .error)" != "xnull" ] && [ "x$(echo $resp | jq -r .error)" != "x" ]
	#then		
	#	echo "[WARNING] error durning upload"
	#	echo "$(echo $resp | jq -r .message)"
	#fi
	#if [ "x$(echo $resp | jq -r .error)" == "x" ]
	#then		
	#	echo "[INFO] File uploaded"
	#fi
}


function subscribe (){
    verifySubscribePrerequisits
    
    $Credentials = "${$DEPLOY_USER}:${$DEPLOY_PASSWORD}"
	$Bytes = [System.Text.Encoding]::Unicode.GetBytes($Text)
    $authorization =[Convert]::ToBase64String($Bytes)

	
	Write-Output "[INFO] Tenant $DEPLOY_TENANT subscription to application $APPLICATION_NAME with id $APPLICATION_ID"
    $body="{""application"":{""id"": ""$APPLICATION_ID""}}"


    $Result = Invoke-RestMethod -Method Post -Uri "$DEPLOY_ADDRESS/tenant/tenants/$DEPLOY_TENANT/applications" -Headers @{Authorization=("Basic {0}" -f $authorization)} -Body $body -ContentType "application/json" -ErrorVariable RestError -ErrorAction "SilentlyContinue"
            
    if ($RestError)
    {
        $HttpStatusCode = $RestError.ErrorRecord.Exception.Response.StatusCode.value__
        $HttpStatusDescription = $RestError.ErrorRecord.Exception.Response.StatusDescription
    
        Write-Output "[WARNING] error subscribing tenant to application "
        Write-Output "Http Status Code: $($HttpStatusCode) `nHttp Status Description: $($HttpStatusDescription)"
    }else{
        Write-Output "[INFO] Tenant $DEPLOY_TENANT subscribed to application $APPLICATION_NAME"
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

