Param(    
    [alias("t")]
    [parameter(Mandatory = $false)] [string]$tenant,

    [alias("u")]
    [parameter(Mandatory = $false)] [string]$username,

    [alias("p")]
    [parameter(Mandatory = $false)] [string]$password,

    [alias("an")]
    [parameter(Mandatory = $false)] [string]$appname,

    [alias("f")]
    [parameter(Mandatory = $false)] [string]$file
)

function Get-IniFile 
{  
    param(  
        [parameter(Mandatory = $true)] [string] $filePath  
    )  

    $anonymous = "NoSection"

    $ini = @{}  
    switch -regex -file $filePath  
    {  
        "^\[(.+)\]$" # Section  
        {  
            $section = $matches[1]  
            $ini[$section] = @{}  
            $CommentCount = 0  
        }  

        "^(;.*)$" # Comment  
        {  
            if (!($section))  
            {  
                $section = $anonymous  
                $ini[$section] = @{}  
            }  
            $value = $matches[1]  
            $CommentCount = $CommentCount + 1  
            $name = "Comment" + $CommentCount  
            $ini[$section][$name] = $value  
        }   

        "(.+?)\s*=\s*(.*)" # Key  
        {  
            if (!($section))  
            {  
                $section = $anonymous  
                $ini[$section] = @{}  
            }  
            $name,$value = $matches[1..2]  
            $ini[$section][$name] = $value  
        }  
    }  

    return $ini  
}  

function getResponseAppNameJson($username,$pass,$site,$appname) {

		$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$pass)))
		$requestAppName = "http://$site/application/applicationsByName/$appname"
		$responseAppNameJson =Invoke-WebRequest $requestAppName  -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ErrorAction SilentlyContinue | ConvertFrom-Json
        

      return $responseAppNameJson
}

function getAppId($response) {

	$appid = 0

	if($response)
	{
	 $app = $response.applications | where {$_.name -eq $appname } 
	 if($app)
	 {
		$appid = $app.id
	 }
	}

	if($appid -eq 0){
		throw "Application does not exist."
	}
return $appid;
}

function Invoke-MultipartFormDataUpload
{
    PARAM
    (
        [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$InFile,
        [string]$ContentType,
        [Uri][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$Uri,
        [string] [parameter(Mandatory = $true)] $Header
    )
    BEGIN
    {
        if (-not (Test-Path $InFile))
        {
            $errorMessage = ("File {0} missing or unable to read." -f $InFile)
            $exception =  New-Object System.Exception $errorMessage
			$errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, 'MultipartFormDataUpload', ([System.Management.Automation.ErrorCategory]::InvalidArgument), $InFile
			$PSCmdlet.ThrowTerminatingError($errorRecord)
        }

        if (-not $ContentType)
        {
            Add-Type -AssemblyName System.Web

            $mimeType = [System.Web.MimeMapping]::GetMimeMapping($InFile)
            
            if ($mimeType)
            {
                $ContentType = $mimeType
            }
            else
            {
                $ContentType = "application/octet-stream"
            }
        }
    }
    PROCESS
    {
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

		try
		{
			return Invoke-WebRequest -Uri $Uri `
									 -Method Post `
									 -ContentType "multipart/form-data; boundary=$boundary" `
									 -Body $body `
									 -Headers @{Authorization=("Basic {0}" -f $Header)}
		}
		catch [Exception]
		{
			$PSCmdlet.ThrowTerminatingError($_)
		}
    }
    END { }
}

if (!$file -and !$tenant -and !$username -and !$password -and !$appname) { 
#1. Just call deploy.ps1
#a. The script looks for a “settings.ini” in the same directory. If found, uses the credentials and tenant URL from that file
#b. If settings.ini is not found, an error is shown 
    
    Write-Host "case 1"
	
	$file = "settings.ini"
	$isLocalFile = $true
	
	if (Test-Path $file) { 
		$isLocalFile = $true;
	}else{
	
	    $isLocalFile = $false
		$appdata = Get-Childitem env:APPDATA | %{ $_.Value }
		$file = "$appdata\c8y\$file"
		
		if (-not(Test-Path $file)) { 
			throw [System.IO.FileNotFoundException] "$file not found."
		}
	}
	
	if(-not($isLocalFile))
	{
		$settingsIni = Get-IniFile $file
	}else{
	    $settingsIni = Get-IniFile .\$file
	}
	
    $username = $settingsIni.deploy.username
    $password = $settingsIni.deploy.password
    $tenant = $settingsIni.deploy.tenant
    $appname = $settingsIni.deploy.appname
	
    Write-Host $username
    Write-Host $tenant
    Write-Host $appname

    	try 
		{
		$appid = 0
		$responseJson = ""
		$responseJson = getResponseAppNameJson $username $password $site $appname
		$appid = getAppId $responseJson

		$uri = "http://$($site)/application/applications/$($appid)/binaries"		 
		$filePath = Resolve-Path -Path ".\images\multi\image.zip"
		
		Invoke-MultipartFormDataUpload -InFile $filePath -Uri $uri -Header $base64AuthInfo
		
		Write-Host "I'm done!"
		
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
ElseIf($file -and !$tenant -and !$username -and !$password -and !$appname)
{
    Write-Host "case 2"
	
	$isLocalFile = $true
	
	if (Test-Path $file) { 
		$isLocalFile = $true;
	}else{
	
	    $isLocalFile = $false
		$appdata = Get-Childitem env:APPDATA | %{ $_.Value }
		$file = "$appdata\c8y\$file"
		
		if (-not(Test-Path $file)) { 
			throw [System.IO.FileNotFoundException] "$file not found."
		}
	}
	
	if(-not($isLocalFile))
	{
		$settingsIni = Get-IniFile $file
	}else{
	    $settingsIni = Get-IniFile .\$file
	}
	

    $settingsIni = Get-IniFile .\$file

    $username = $settingsIni.deploy.username
    $password = $settingsIni.deploy.password
    $tenant = $settingsIni.deploy.tenant
    $appname = $settingsIni.deploy.appname

    Write-Host $username
    Write-Host $tenant
    Write-Host $appid

        try {
			$appid = 0
			$responseJson = ""
			$responseJson = getResponseAppNameJson $username $password $site $appname
			$appid = getAppId $responseJson
			
			$uri = "http://$($site)/application/applications/$($appid)/binaries"		 
			$filePath = Resolve-Path -Path ".\images\multi\image.zip"

			Invoke-MultipartFormDataUpload -InFile $filePath -Uri $uri -Header $base64AuthInfo
		
		Write-Host "I'm done!"
		
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
ElseIf($file -and ($tenant -or $username -or $password -or $appname))
{
    Write-Host "case 3"
	
	if (Test-Path $file) { 
		$isLocalFile = $true;
	}else{
	
	    $isLocalFile = $false
		$appdata = Get-Childitem env:APPDATA | %{ $_.Value }
		$file = "$appdata\c8y\$file"
		
		if (-not(Test-Path $file)) { 
			throw [System.IO.FileNotFoundException] "$file not found."
		}
	}
	
	if(-not($isLocalFile))
	{
		$settingsIni = Get-IniFile $file
	}else{
	    $settingsIni = Get-IniFile .\$file
	}
	

    $settingsIni = Get-IniFile .\$file

    if(!$tenant){
        $tenant = $settingsIni.deploy.tenant
    }
    if(!$username){
        $username = $settingsIni.deploy.username
    }
    if(!$password){
        $password = $settingsIni.deploy.password
    }
    if(!$appname){
        $appname = $settingsIni.deploy.$appname
    }

       try {
		$appid = 0
		$responseJson = ""
		$responseJson = getResponseAppNameJson $username $password $site $appname
		$appid = getAppId $responseJson
		
		if($appid -eq 0){
			throw "Application does not exist."
		}
		#		
		$uri = "http://$($site)/application/applications/$($appid)/binaries"		 
		$filePath = Resolve-Path -Path ".\images\multi\image.zip"
		
		Write-Output $base64AuthInfo;
		Write-Output $filePath;
		
		Invoke-MultipartFormDataUpload -InFile $filePath -Uri $uri -Header $base64AuthInfo
		
		Write-Host "I'm done!"
		
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
Else
{
    $File_Path_Error = [string]"The file path is not valid"
    Write-Error $File_Path_Error
}


