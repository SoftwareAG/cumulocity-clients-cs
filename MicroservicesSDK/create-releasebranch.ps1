[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
    hg branch "$version"
    hg add
    hg commit -m "prepare release $version"
    hg tag "$version"
	hg push -f
}