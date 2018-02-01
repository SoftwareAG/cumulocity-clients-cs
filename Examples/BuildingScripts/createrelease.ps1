[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
	hg tag "bsv$version"
	hg push
}