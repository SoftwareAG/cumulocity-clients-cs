function ReadCommitCountInBranch
(
    [string] $branch
)
{

	$commitCountInBranch = ((& hg log -b $branch --template "." ) | measure-object -line).Lines
	Write-Host "ReadCommitCountInBranch: $commitCountInBranch"
	return $commitCountInBranch;
}

function ReadBranchName
{
    $branchName = (& hg branch).Trim()
    return $branchName
}

function ReadIsLastTagCommit
{
    Try
    {
		$lasttag = (& hg log -r "last(tag('re:prerelease\d*'))" --template "{tags}\n").Trim()
		
		
		if($lasttag.Length -eq 0)
		{
			return "r0.0.0"    
		}
		return $lasttag.Substring(10,$lasttag.Length-10)
	}Catch
    {
	   return "r0.0.0"
	}
}

function CreateReleaseBranch
{
	$versionFileName = ".\version.props"
	$VersionExist = Test-Path -Path $versionFileName

	if ($VersionExist -eq $true) {
		Write-Host "Yes The version file exists"
		Remove-Item $versionFileName
	}

	New-Item $versionFileName -ItemType file

    $CurrentBranch = ReadBranchName
	$LastTagCommit = ReadIsLastTagCommit -branch $ReadBranchName
	$ReleaseBranch = "release/$LastTagCommit"	
	if($CurrentBranch.StartsWith("release/")){
		$ReleaseBranch = $CurrentBranch
	}
	$CommitCountInBranch = ReadCommitCountInBranch  -branch $ReleaseBranch
	
	Write-Host "ReleaseBranch: $ReleaseBranch"
	Write-Host "LastTagCommit: $LastTagCommit"
	Write-Host "ReadCommitCountInBranch: $CommitCountInBranch"
	
	"CurrentBranch:$CurrentBranch" | Add-Content $versionFileName
	"LastTagCommit:$LastTagCommit" | Add-Content $versionFileName
	"ReleaseBranch:$ReleaseBranch" | Add-Content $versionFileName
	"ReadCommitCountInReleaseBranch:$CommitCountInBranch" | Add-Content $versionFileName

	if( ($CurrentBranch -eq "default") -And ( $CommitCountInBranch -eq  0) -And ( $LastTagCommit -ne  "r0.0.0")  )
	{
		# hg up $LastTagCommit 
	    # hg flow release start $LastTagCommit
		# hg push
	}
	elseif($CurrentBranch.StartsWith("release/") )
	{
		$major,$minor,$rev = $LastTagCommit.split('.')
		if($LastTagCommit -eq  "r0.0.0" )
		{
		  $branch, $tag = $CurrentBranch.split('/') 
		  $LastTagCommit =$tag 
		  $major,$minor,$rev = $LastTagCommit.split('.')
		}
		$rev = $rev/1 + 1
		

		# hg tag "$major.$minor.$rev"
		# hg push
	}

}

CreateReleaseBranch