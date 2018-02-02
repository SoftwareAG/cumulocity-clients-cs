[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
	hg tag "mssdkv$version"
	hg push
}