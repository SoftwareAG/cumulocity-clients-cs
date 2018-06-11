[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
    $version  = "$version"
    hg add
    hg commit -m "prepare hotfix $version"
    hg tag "$version"
	#git push
}