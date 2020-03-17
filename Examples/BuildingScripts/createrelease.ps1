[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
	git tag "bsv$version"
	git push
}