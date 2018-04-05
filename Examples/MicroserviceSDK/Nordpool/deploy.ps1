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

try {

	$username ='management/piotr'
	$password ='piotr3333'
	$site ="management.staging7.c8y.io"
	$id = "1070"
	
	 #$username ='schenck/pnowak'
	 #$password ='thx4support'
	 #$site ="schenck.adamos-dev.com"
	 #$id = "3295"
	
	$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))
	
	$uri = "http://$($site)/application/applications/$($id)/binaries"
	 
    $filePath = Resolve-Path -Path ".\images\multi\image.zip"
    
    Write-Output $base64AuthInfo;
    Write-Output $filePath;
	
    Invoke-MultipartFormDataUpload -InFile $filePath -Uri $uri -Header $base64AuthInfo
    
	Write-Host "I'm done!"
	
	} catch {
    # Dig into the exception to get the Response details.
    # Note that value__ is not a typo.
    Write-Host "StatusCode:"  $_.Exception.Message
	
    #    Write-Host "Response:" $_.Exception.Response 
    #    $result = $_.Exception.Response.GetResponseStream()
    #    $reader = New-Object System.IO.StreamReader($result)
    #    $reader.BaseStream.Position = 0
    #    $reader.DiscardBufferedData()
    #    $responseBody = $reader.ReadToEnd();
    #	 Write-Host "ResponseBody:"  $responseBody
}