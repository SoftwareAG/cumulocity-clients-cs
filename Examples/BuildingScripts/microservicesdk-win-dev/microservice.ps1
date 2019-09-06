<#
.SYNOPSIS
    Script to pack, deploy and subscribe the application in a single line.

.DESCRIPTION
    Cumulocity provides you with an utility tool for easy microservice packaging, deployment and subscription. The script requires running docker.
    https://docs.adamos.com/guides/reference/microservice-package/

.PARAMETER mode
    pack deploy subscribe -n hello-world -d {url} -u {username} -p {password} -te {tenant}

.EXAMPLE
    microservice.ps1 pack deploy subscribe -n hello-world -d {url} -u {username} -p {password} -te {tenant}
    microservice.ps1 subscribe -n hello-world -d {url} -u {username} -p {password} -te {tenant} -id {applicationId}
    microservice.ps1 deploy -n hello-world -d {url} -u {username} -p {password} -te {tenant}
#>

$global:WORK_DIR = $(pwd)
$global:IMAGE_NAME = ""
$global:TAG_NAME = "latest"
$global:DEPLOY_ADDRESS = ""
$global:DEPLOY_TENANT = ""
$global:DEPLOY_USER = ""
$global:DEPLOY_PASSWORD = ""
$global:APPLICATION_NAME = ""
$global:APPLICATION_ID = ""
$global:ZIP_NAME = ""

$global:PACK = 1
$global:DEPLOY = 1
$global:SUBSCRIBE = 1
$global:HELP = 1


function execute {
    [CmdletBinding()]
    Param(
        [Object[]]$args
    )


    readInput $args

    cd $WORK_DIR
    if ("$HELP".equals("0")) {
        printHelp;
        exit;
    }

    if ( "$PACK".equals("1") -and "$DEPLOY".equals("1") -and "$SUBSCRIBE".equals("1")) {
        Write-Output "[INFO] No goal set. Please set pack, deploy or subscribe"
    }

    if ("$PACK".equals("0")) {
        Write-Output "[INFO] Start packaging"
        verifyPackPrerequisits
        clearTarget
        buildImage
        exportImage
        zipFile
        Write-Output "[INFO] End packaging"
    }
    if ( "$DEPLOY".Equals("0")) {
        Write-Output "[INFO] Start deployment"
        deploy
        Write-Output "[INFO] End deployment"
    }
    if ("$SUBSCRIBE".Equals("0")) {
        Write-Output  "[INFO] Start subsciption"
        subscribe
        Write-Output  "[INFO] End subsciption"
    }
    exit;
}


function readInput {
    [CmdletBinding()]
    Param(
        [Object[]]$args
    )

    Write-Output "[INFO] Read input"

    For ($i = 0; $i -le $args.Count; $i++) {
        if ($args[$i] -eq "pack") {
            $global:PACK = 0;
        }
        if ($args[$i] -eq "deploy") {
            $global:DEPLOY = 0;
        }
        if ($args[$i] -eq "subscribe") {
            $global:SUBSCRIBE = 0;
        }
        if (($args[$i] -eq "help") -or ($args[$i] -eq "--help")) {
            $global:HELP = 0;
        }
        if (($args[$i] -eq "-dir") -or ($args[$i] -eq "--directory")) {
            $global:WORK_DIR = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-n") -or ($args[$i] -eq "--name")) {
            $global:IMAGE_NAME = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-t") -or ($args[$i] -eq "--tag")) {
            $global:TAG_NAME = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-d") -or ($args[$i] -eq "--deploy")) {
            $global:DEPLOY_ADDRESS = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-u") -or ($args[$i] -eq "--user")) {
            $global:DEPLOY_USER = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-p") -or ($args[$i] -eq "--password")) {
            $global:DEPLOY_PASSWORD = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-te") -or ($args[$i] -eq "--tenant")) {
            $global:DEPLOY_TENANT = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-a") -or ($args[$i] -eq "--application")) {
            $global:APPLICATION_NAME = $args[$i + 1]
            $i = $i + 1
        }
        if (($args[$i] -eq "-id") -or ($args[$i] -eq "--applicationId")) {
            $global:APPLICATION_ID = $args[$i + 1]
            $i = $i + 1
        }
    }

    setDefaults
}

function setDefaults() {
    $global:ZIP_NAME = "$global:IMAGE_NAME.zip"

    if ( "x$APPLICATION_NAME".Equals("x")) {
        $global:APPLICATION_NAME = $global:IMAGE_NAME
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
    Write-Output "[INFO] Check input"

    Set-Variable -Name result -Option AllScope
    $result = 0
    verifyParamSet "$IMAGE_NAME" "name"

    isPresent  @($((Get-ChildItem -Filter docker -Directory -Recurse -Depth 1 | measure-object -line).Lines), "[ERROR] Stopped: missing docker directory in work directory: $WORK_DIR")
    isPresent  @($((Get-ChildItem -Filter Dockerfile -File -Recurse -Depth 1 | measure-object -line).Lines), "[ERROR] Stopped: missing dockerfile in work directory: $WORK_DIR")
    isPresent  @($((Get-ChildItem -Filter cumulocity.json -File -Recurse -Depth 1 | measure-object -line).Lines), "[ERROR] Stopped: missing cumulocity.json in work directory: $WORK_DIR")

    if ( "$result".Equals("1")) {
        Write-Output "[WARNING] Pack skiped"
        exit 1
    }
}

function isPresent() {
    [CmdletBinding()]
    Param(
        [Object[]]$args
    )
    Write-Output "[INFO] Check input"

    $present = $args[0]

    if (-not "$present".Equals("1")) {
        Write-Output $args[1]
        $result = 1
    }
}

function clearTarget() {
    Write-Output "clearTarget ZIP: $ZIP_NAME"
    Write-Output "[INFO] Clear target files.$ZIP_NAME."

    if (Test-Path "image.tar") {
        Remove-Item "image.tar"
    }
    if (Test-Path "$global:ZIP_NAME") {
        Remove-Item "$global:ZIP_NAME"
    }
}

function buildImage() {
    $imagename = $global:IMAGE_NAME
    $imagename = $imagename + ":"
    $imagename = $imagename + $global:TAG_NAME

    Set-Location .\docker
    Write-Output "[INFO] Build image $imagename."
    docker build -t $imagename .
    Set-Location ..
}

function  exportImage() {
    $imagename = $global:IMAGE_NAME
    $imagename = $imagename + ":"
    $imagename = $imagename + $global:TAG_NAME
    Write-Output "[INFO] Export image"
    docker save $imagename > "docker\image.tar"
}

function zipFile() {
    Get-Location
    Write-Output "[INFO] Zip file $global:ZIP_NAME"
    Compress-Archive -Path .\docker\* -CompressionLevel Fastest -DestinationPath .\$global:ZIP_NAME
}
$deployResult = 0
function verifyDeployPrerequisits() {

    verifyParamSet "$IMAGE_NAME" "name"
    verifyParamSet "$DEPLOY_ADDRESS" "address"
    verifyParamSet "$DEPLOY_TENANT" "tenant"
    verifyParamSet "$DEPLOY_USER" "user"
    verifyParamSet "$DEPLOY_PASSWORD" "password"

    if ("$deployResult".Equals("1")) {
        Write-Output "[WARNING] Deployment skiped"
        exit 1
    }
}

function verifyParamSet() {
    [CmdletBinding()]
    Param(
        [string]$param1,
        [string]$param2
    )

    Write-Output "x$param1"

    if ("x$param1".Equals("x")) {
        Write-Output "[WARNING] Missing parameter: $param2"
        $deployResult = 1
    }
}

function push() {

    getApplicationId

    if ( "x$global:APPLICATION_ID".Equals("x")) {
        Write-Output "[INFO] Application with name $APPLICATION_NAME not found, add new application"
        createApplication     
        getApplicationId

        if ("x$global:APPLICATION_ID".Equals("x")) {
            Write-Output  "[ERROR] Could not create application"
            EXIT 1
        }
    }

    Write-Output "aaa authorization $global:APPLICATION_ID"
    uploadFile
}

function getApplicationId() {

	$authorization = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $global:DEPLOY_USER,$global:DEPLOY_PASSWORD)))
  
    # Write-Output "ApplicationId Uri $DEPLOY_ADDRESS/application/applicationsByName/$APPLICATION_NAME"
    # Write-Output "ApplicationId authorization $authorization"

    $Result = Invoke-RestMethod -Uri "$DEPLOY_ADDRESS/application/applicationsByName/$APPLICATION_NAME" `
                                -Headers @{Authorization = ("Basic {0}" -f $authorization)} `
                                -ErrorVariable RestError -ErrorAction "SilentlyContinue"

    if ($RestError) {
        $HttpStatusCode = $RestError.ErrorRecord.Exception.Response.StatusCode.value__
        $HttpStatusDescription = $RestError.ErrorRecord.Exception.Response.StatusDescription

        Write-Output "[ERROR] Error while connecting to platform"
        Write-Output "Http Status Code: $($HttpStatusCode) `nHttp Status Description: $($HttpStatusDescription)"
        Exit
    }
    else {
        $global:APPLICATION_ID = $Result.applications.id
    }
}

function  createApplication() {

	$authorization = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $global:DEPLOY_USER,$global:DEPLOY_PASSWORD)))
    $body = "{
			""name"": ""$APPLICATION_NAME"",
			""type"": ""MICROSERVICE"",
			""key"":  ""$APPLICATION_NAME-microservice-key""
		    }";

    $Result = Invoke-RestMethod -Method Post -Uri  "$DEPLOY_ADDRESS/application/applications" `
                                -Headers @{Authorization = ("Basic {0}" -f $authorization)} `
								-ContentType "application/json" ` 
                                -ErrorVariable RestError -ErrorAction "SilentlyContinue" `
								-Body $body
}


function deploy() {
    verifyDeployPrerequisits
    push
}

function uploadFile() {

    Write-Output "[INFO] Upload file $WORK_DIR\$ZIP_NAME"

	$authorization = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $global:DEPLOY_USER,$global:DEPLOY_PASSWORD)))
    $uri = [string]::Concat($global:DEPLOY_ADDRESS, "/application/applications/", $global:APPLICATION_ID,"/binaries")

    Write-Output "[INFO] uri-> $authorization"
    Write-Output "[INFO] global:DEPLOY_ADDRESS-> $global:DEPLOY_ADDRESS"
    Write-Output "[INFO] global:APPLICATION_ID-> $global:APPLICATION_ID"

    try {
        
        Invoke-MultipartFormDataUpload -InFile "$WORK_DIR\$ZIP_NAME" `
                                       -Uri $uri `
                                       -Header $authorization 
    }
    catch {
        # Dig into the exception to get the Response details.
        # Note that value__ is not a typo.
        Write-Host "StatusCode:"  $_.Exception.Message

        Write-Host "Response:" $_.Exception.Response
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd();
        Write-Host "ResponseBody:"  $responseBody
    }
}


function subscribe () {
    verifySubscribePrerequisits

	$authorization = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $global:DEPLOY_USER,$global:DEPLOY_PASSWORD)))

    Write-Output "[INFO] authorization { $authorization}"
    Write-Output "[INFO] Tenant $DEPLOY_TENANT subscription to application $APPLICATION_NAME with id $APPLICATION_ID"
    $body = "{""application"":{""id"": ""$APPLICATION_ID""}}"

    $Result = Invoke-RestMethod -Method Post -Uri "$DEPLOY_ADDRESS/tenant/tenants/$DEPLOY_TENANT/applications" `
                    -Headers @{Authorization = ("Basic {0}" -f $authorization)} `
                    -Body $body `
                    -ContentType "application/json" -ErrorVariable RestError -ErrorAction "SilentlyContinue"


    if ($RestError) {
        $HttpStatusCode = $RestError.ErrorRecord.Exception.Response.StatusCode.value__
        $HttpStatusDescription = $RestError.ErrorRecord.Exception.Response.StatusDescription

        Write-Output "[WARNING] error subscribing tenant to application "
        Write-Output "Http Status Code: $($HttpStatusCode) `nHttp Status Description: $($HttpStatusDescription)"
    }
    else {
        Write-Output "[INFO] Tenant $DEPLOY_TENANT subscribed to application $APPLICATION_NAME"
    }

}

function  verifySubscribePrerequisits() {
    if ("x$APPLICATION_ID".Equals("x")) {
        Write-Output "[ERROR] Subscription not possible uknknown applicaitonId"
        exit 1
    }
    verifyDeployPrerequisits
}

function Invoke-MultipartFormDataUpload {
    PARAM
    (
        [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$InFile,
        [string]$ContentType,
        [Uri][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$Uri,
        [string] [parameter(Mandatory = $true)] $Header
    )
    BEGIN {
        if (-not (Test-Path $InFile)) {
            $errorMessage = ("File {0} missing or unable to read." -f $InFile)
            $exception = New-Object System.Exception $errorMessage
            $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, 'MultipartFormDataUpload', ([System.Management.Automation.ErrorCategory]::InvalidArgument), $InFile
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }

        if (-not $ContentType) {
            Add-Type -AssemblyName System.Web

            $mimeType = [System.Web.MimeMapping]::GetMimeMapping($InFile)

            if ($mimeType) {
                $ContentType = $mimeType
            }
            else {
                $ContentType = "application/octet-stream"
            }
        }
    }
    PROCESS {
        $fileName = Split-Path $InFile -leaf
        $boundary = [guid]::NewGuid().ToString()

        $fileBin = [System.IO.File]::ReadAllBytes($InFile)
        $enc = [System.Text.Encoding]::GetEncoding("iso-8859-1")

$template = @'
--{0}
Content-Disposition: form-data; name="fileData"; filename="{1}"
Content-Type: {2}

{3}
--{0}--

'@

        $body = $template -f $boundary, $fileName, $ContentType, $enc.GetString($fileBin)
        

        #Write-Output "[INFO] body: $body"
        Write-Output "[INFO] boundary: $boundary"

        try {
            return Invoke-WebRequest -Uri $Uri `
                -Method Post `
                -ContentType "multipart/form-data; boundary=$boundary" `
                -Body $body `
                -Headers @{Authorization = ("Basic {0}" -f $Header)}
        }
        catch [Exception] {
            $PSCmdlet.ThrowTerminatingError($_)
        }
    }
    END { }
}

execute $args

