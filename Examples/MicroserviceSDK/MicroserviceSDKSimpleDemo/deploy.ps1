try {
	$username ='management/admin'
	$password ='Pyi1bo1r'
	$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))
    $uri = 'http://management.staging7.c8y.io/application/applications/38/binaries'
    $filePath = Resolve-Path -Path ".\images\hello-world-multi\image.zip"
    
    Write-Output $base64AuthInfo;
    Write-Output $filePath;

    Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Uri $uri -Method Post -InFile $filePath -ContentType "multipart/form-data"
} catch {
    Write-Host "StatusCode:"  $_.Exception.Message
}
