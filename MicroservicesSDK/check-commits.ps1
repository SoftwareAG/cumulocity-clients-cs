[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)][string]$tag
)

$countCommitsFileName = ".\commits.props"


$VersionExist = Test-Path -Path $countCommitsFileName

if ($VersionExist -eq $true) {
	Write-Host "Yes The version file exists"
	Remove-Item $countCommitsFileName
}

New-Item $countCommitsFileName -ItemType file

Try
{
   $lines = (& hg log --template "{rev}\n" -r"prerelease$tag":: | measure-object -line).Lines
   [int]$commits = $lines-2

      if($commits -gt 0){
            #hg up "prerelease$tag"           
       }else{
            
       }
  
   "$commits" | Add-Content $countCommitsFileName


}Catch
{

}