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
   $lines = (& git tag --list 'prerelease'$tag :: | sort -V | measure-object -line).Lines
   Write-Host "Lines $lines"
   [int]$commits = $lines-2

      if($commits -gt 0){
            #git checkout "prerelease$tag"           
       }else{
            
       }
  
   "$commits" | Add-Content $countCommitsFileName


}Catch
{

}