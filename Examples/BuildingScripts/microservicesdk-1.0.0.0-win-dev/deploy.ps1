try {
	$username ='username'
	$password ='pass'
	$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))
    $uri = 'http://url/application/applications/01/binaries'
    $filePath = Resolve-Path -Path ".\images\multi\image.zip"
    
    Write-Output $base64AuthInfo;
    Write-Output $filePath;

    Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Uri $uri -Method Post -InFile $filePath -ContentType "multipart/form-data"
} catch {
    # Dig into the exception to get the Response details.
    # Note that value__ is not a typo.
    Write-Host "StatusCode:"  $_.Exception.Message
}
