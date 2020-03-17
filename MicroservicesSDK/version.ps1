
[CmdletBinding()]
Param(
    [string] $local = "true",
    [string] $branch = $null
)

function ReadBuildProps
{
    $currentDir = Get-Location	
	$filePath = "$currentDir/version.props"
	
	if (Test-Path $filePath){
	  Write-Host "version.props was deleted."
	  Remove-Item $filePath
	}
	
	$filePath = "$currentDir/build.props"		
	if (-not (Test-Path $filePath) -or (-not (IsRepository)) )  {
		Write-Host "build.props not found or repository not found"
		Break
	}else
	{   		
		$buildProps = ConvertFrom-StringData (Get-Content ./build.props -raw)
		$buildObject = New-Object PSObject -Property $buildProps | Select-Object versionMajor, versionMinor, versionRev
		return $buildObject
	}
}
function ReadBranchName
{
    $branchName = (& git branch).Trim()
    return $branchName
}
function ReadCommitCount
{
    $count = (& git rev-list --all --count).Trim()
    return $count
}

function ReadIsLastTagCommit
{
    Try
    {
		$lasttag = (& git tag  --list 'mssdkv*' | sort -V | tail -1).Trim()
		
		
		if($lasttag.Length -eq 0)
		{
			return "0.0.0"   
		}
		return $lasttag.Substring(3,$lasttag.Length-3)
	}Catch
    {
	   return "0.0.0"
	}
}
#function IsRepository
#{
#	$currentDir = Get-Location	
#	$dirPath = "$currentDir/.git"
	
#	if(!(Test-Path -Path $dirPath )){
#		return $false
#	}	
#	return $true
#}

function IsRepository
{
    $value = (& git status);
	
	if($value -eq $null){
		return $false;
	}
	
    $string = ((& git  status).Trim() -split '\n')[0]
	
	if ( $string -contains '*abort:*') {
		return $false;
	}
	else{
		return $true;
	}
    
}

function CreateVersion
(
    [string] $major,
    [string] $minor,
    [string] $rev,
    [string] $branch,
    [int] $commits,
    [bool] $local = $true,
	[string] $istagged
)
{
    $version = [string]::Concat($major, ".", $minor, ".", $rev)
    $prerelease = $null

    $branch = $branch.Replace('/', '-')

    if ($branch -ne "default")
    {
        if ($local)
        {
            $prerelease = $branch
        }
        else
        {
            $prerelease = "$($branch)-$($commits)"
        }
    }

    if ($prerelease -ne $null) 
    {
        $semVer = [string]::Concat($version, "-", $prerelease)
    }
    else
    {
        $semVer = $version
    }

    return @{
        Version = $version
        SemanticVersion = $semVer
        PreRelease = $prerelease
		IsTagged = $istagged 
    }
}

function WriteVersion
(
    [string] $version,
    [string] $semanticVersion,
    [string] $prerelease,
	[string] $isTagged
)
{
    Set-Content ./version.props "version=$($version)`nsemanticVersion=$($semanticVersion) `nistagged=$($isTagged) `nprerelease=$($prerelease)"
}

function ResolveVersion
(
    [bool] $local = $true
)
{
    $parts = ReadBuildProps

    if (!$branch)
    {
        $branch = ReadBranchName
    }    
			
    $count = ReadCommitCount
	$tag = ReadIsLastTagCommit
	
	Write-Host "ReadIsLastTagCommit"
	Write-Host $tag
    Write-Host "ReadIsLastTagCommit"
	
    $version = CreateVersion -major $parts.versionMajor -minor $parts.versionMinor -rev $parts.versionRev -branch $branch -commits $count -local $local -istagged $tag

	Write-Host $version.Version
	Write-Host $version.SemanticVersion
	Write-Host $version.PreRelease
	Write-Host $version.IsTagged
	
    WriteVersion -version $version.Version -semanticVersion $version.SemanticVersion -prerelease $version.PreRelease -isTagged $version.IsTagged
}

$out = $null
if ([bool]::TryParse($local, [ref]$out)) {
    # parsed to a boolean
    Write-Host "Value: $out"
	ResolveVersion -local $out
} else {
    Write-Host "Input is not boolean: $a"
}
