[CmdletBinding()]
Param(
    [string] $version = $null
)

	
if($version)
{
    git branch "$version"
    git add
    git commit -m "prepare release $version"
    #git tag "$version"
	git push -f
}