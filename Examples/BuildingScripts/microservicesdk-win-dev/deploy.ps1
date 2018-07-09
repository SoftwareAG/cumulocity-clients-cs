Param(    
    [alias("s")]
    [parameter(Mandatory = $false)] [string]$url,

    [alias("u")]
    [parameter(Mandatory = $false)] [string]$username,

    [alias("p")]
    [parameter(Mandatory = $false)] [string]$password,

    [alias("an")]
    [parameter(Mandatory = $false)] [string]$appname,

    [alias("f")]
    [parameter(Mandatory = $false)] [string]$file
)

function Select-Value {
  param
  (
    [Parameter(Mandatory=$true, ValueFromPipeline=$true, HelpMessage="Data to process")]
    $InputObject
  )
  process {
     $InputObject.Value 
  }
}
function Get-IniFile {  
    param(  
        [parameter(Mandatory = $true)] [string] $filePath  
    )  

    $anonymous = "NoSection"

    $ini = @{}  
    switch -regex -file $filePath {  
        "^\[(.+)\]$" # Section   {  
            $section = $matches[1]  
            $ini[$section] = @{}  
            $CommentCount = 0  
        }  

        "^(;.*)$" # Comment   {  
            if (!($section)) {  
                $section = $anonymous  
                $ini[$section] = @{}  
            }  
            $value = $matches[1]  
            $CommentCount = $CommentCount + 1  
            $name = "Comment" + $CommentCount  
            $ini[$section][$name] = $value  
        }   

        "(.+?)\s*=\s*(.*)" # Key   {  
            if (!($section)) {  
                $section = $anonymous  
                $ini[$section] = @{}  
            }  
            $name,$value = $matches[1..2]  
            $ini[$section][$name] = $value  
        }  
    }  

    return $ini  
}  

function getResponseAppNameJson([Parameter(Mandatory=$true)]$username,[Parameter(Mandatory=$true)]$pass,[Parameter(Mandatory=$true)]$site,[Parameter(Mandatory=$true)]$appname) {
    $aid = 0
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$pass)))
    $requestAppName = "http://$site/application/applicationsByName/$appname"
    $responseAppNameJson =Invoke-WebRequest -Uri $requestAppName  -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ErrorAction SilentlyContinue | ConvertFrom-Json            

    if($responseAppNameJson) {
     $app = (($responseAppNameJson).applications  | Where-Object {$_.name -eq $appname})
     if($app) {
      $aid = $app.id
     }
    }
    if($aid-eq 0){
      throw "Application does not exist."
    }
    return $aid
}
function Invoke-DataUpdate {
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$false, Position=0)]
    [System.Int32]
    $appid = 0,
    
    [Parameter(Mandatory=$false, Position=1)]
    [System.String]
    $responseJson = ""
  )
  
  $id = getResponseAppNameJson $username $password $url $appname
  
  if($id -eq 0){
    throw "Application does not exist."
  }
  #		
  $uri = "http://$($url)/application/applications/$($id)/binaries"		 
  $filePath = Resolve-Path -Path ".\images\multi\image.zip"
 
  Invoke-MultipartFormDataUpload -InFile $filePath -Uri $uri -Header $base64AuthInfo
  
  Write-Host "I'm done!"
}

function Invoke-MultipartFormDataUpload {
    PARAM
    (
        [string][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$InFile,
        [Parameter(Mandatory=$true)][string]$ContentType,
        [Uri][parameter(Mandatory = $true)][ValidateNotNullOrEmpty()]$Uri,
        [string] [parameter(Mandatory = $true)] $Header
    )
    BEGIN {
        if (-not (Test-Path $InFile)) {
            $errorMessage = ("File {0} missing or unable to read." -f $InFile)
            $exception =  New-Object System.Exception $errorMessage
            $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, 'MultipartFormDataUpload', ([Management.Automation.ErrorCategory]::InvalidArgument), $InFile
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }

        if (-not $ContentType) {
            Add-Type -AssemblyName System.Web

            $mimeType = [Web.MimeMapping]::GetMimeMapping($InFile)
            
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
		
      $fileBin = [IO.File]::ReadAllBytes($InFile)
      $enc = [Text.Encoding]::GetEncoding("iso-8859-1")

      $template = @'
--{0}
Content-Disposition: form-data; name="fileData"; filename="{1}"
Content-Type: {2}

{3}
--{0}--

'@

        $body = $template -f $boundary, $fileName, $ContentType, $enc.GetString($fileBin)

      try {
        return Invoke-WebRequest -Uri $Uri `
                     -Method Post `
                     -ContentType "multipart/form-data; boundary=$boundary" `
                     -Body $body `
                     -Headers @{Authorization=("Basic {0}" -f $Header)}
      }
      catch {
        $PSCmdlet.ThrowTerminatingError($_)
      }
    }
    END { }
}

function Write-ErrorMsg($exp) {
  Write-Host "StatusCode:"  $exp.Message 
  #Write-Host "Response:" $exp.Response 
  #$result = $exp.Response.GetResponseStream()
  #$reader = New-Object System.IO.StreamReader($result)
  #$reader.BaseStream.Position = 0
  #$reader.DiscardBufferedData()
  #$responseBody = $reader.ReadToEnd()
  #Write-Host "ResponseBody:"  $responseBody
}

if (!$file -and !$url -and !$username -and !$password -and !$appname) { 
  #1. Just call deploy.ps1
  #a. The script looks for a "settings.ini" in the same directory. If found, uses the credentials and tenant URL from that file
  #b. If settings.ini is not found, an error is shown 
    
	
  $file = "settings.ini"
  $isLocalFile = $true
	
  if (Test-Path $file) { 
    $isLocalFile = $true;
  }
else{
	
      $isLocalFile = $false
    $appdata = Get-Childitem env:APPDATA | Select-Value
    $file = "$appdata\c8y\$file"
		
    if (-not(Test-Path $file)) { 
      throw [IO.FileNotFoundException] "$file not found."
    }
  }
	
  if(-not($isLocalFile)) {
    $settingsIni = Get-IniFile $file
  }
else{
    $settingsIni = Get-IniFile .\$file
  }
	
    $username = $settingsIni.deploy.username
    $password = $settingsIni.deploy.password
    $url = $settingsIni.deploy.url
    $appname = $settingsIni.deploy.appname
	
    Write-Host $username
    Write-Host $url
    Write-Host $appname

    try {    
      $appid = 0
      $responseJson = ""
      Invoke-DataUpdate $appid $responseJson		
    } 
    catch {
      Write-ErrorMsg $_.Exception
    }
}
ElseIf($file -and !$url -and !$username -and !$password -and !$appname) {
	
  $isLocalFile = $true
	
  if (Test-Path $file) { 
    $isLocalFile = $true
  }
  else{
	
    $isLocalFile = $false
    $appdata = Get-Childitem env:APPDATA | Select-Value
    $file = "$appdata\c8y\$file"
		
    if (-not(Test-Path $file)) { 
      throw [IO.FileNotFoundException] "$file not found."
    }
  }
	
  if(-not($isLocalFile)) {
    $settingsIni = Get-IniFile $file
  }
else{
      $settingsIni = Get-IniFile .\$file
  }
	
    $settingsIni = Get-IniFile .\$file

    $username = $settingsIni.deploy.username
    $password = $settingsIni.deploy.password
    $url = $settingsIni.deploy.url
    $appname = $settingsIni.deploy.appname

    Write-Host $username
    Write-Host $url
    Write-Host $appid

    try {
    
      $appid = 0
      $responseJson = ""
      Invoke-DataUpdate $appid $responseJson
    } 
    catch {
      Write-ErrorMsg $_.Exception
     }
}
ElseIf($file -and ($url -or $username -or $password -or $appname)) {
	
  if (Test-Path $file) { 
    $isLocalFile = $true
  }
else{
	
    $isLocalFile = $false
    $appdata = Get-Childitem env:APPDATA | Select-Value
    $file = "$appdata\c8y\$file"
		
    if (-not(Test-Path $file)) { 
      throw [IO.FileNotFoundException] "$file not found."
    }
  }
	
  if(-not($isLocalFile)) {
    $settingsIni = Get-IniFile $file
  }
else{
      $settingsIni = Get-IniFile .\$file
  }
	

    $settingsIni = Get-IniFile .\$file

    if(!$url){
        $url = $settingsIni.deploy.url
    }
    if(!$username){
        $username = $settingsIni.deploy.username
    }
    if(!$password){
        $password = $settingsIni.deploy.password
    }
    if(!$appname){
        $appname = $settingsIni.deploy.appname
    }

    try {
    
      $appid = 0
      $responseJson = ""
      Invoke-DataUpdate $appid $responseJson
		
    } 
    catch [System.Management.Automation.RuntimeException] {
          # get error record
          [Management.Automation.ErrorRecord]$e = $_
           Write-ErrorMsg $e.Exception
        }
}
Else {
    $File_Path_Error = [string]"The file path is not valid"
    Write-Error $File_Path_Error
}


