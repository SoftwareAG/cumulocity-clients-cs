function ReadCommitCountInBranch
(
    [string] $branch
)
{

	$gitlog = (& git log -r "branch('$branch')" )
	
	if ($gitlog -eq $null) 
	{
		return 0;
	}else
	{	
		$commitCountInBranch = git rev-list --count $branch | Measure-Object -line ).Lines
		return $commitCountInBranch ;
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
(
    [string] $branch
)
{
    Try
    {
		$lasttag = (& git tag   --list   | sort -V | tail -1).Trim()
				
		if($lasttag.Length -eq 0)
		{
			return "r0.0.0"   
		}
		return $lasttag.Substring(0,$lasttag.Length)
	}
	Catch
    {
	   return "r0.0.0"
	}
}

function CreateReleaseBranch
{
    $CurrentBranch = ReadBranchName
	$LastTagCommit = ReadIsLastTagCommit -branch $ReadBranchName
	$ReleaseBranch = "release/$LastTagCommit"	
	$CommitCountInBranch = ReadCommitCountInBranch  -branch $ReleaseBranch
	
	Write-Host "ReleaseBranch: $ReleaseBranch"
	Write-Host "LastTagCommit: $LastTagCommit"
	Write-Host "ReadCommitCountInBranch: $CommitCountInBranch"
	
	if( ($CurrentBranch -eq "develop") -And ( $CommitCountInBranch -eq  0) -And ( $LastTagCommit -ne  "r0.0.0")  )
	{
		git checkout  $LastTagCommit
		git checkout -b  release/$LastTagCommit
		git push -u origin release/$LastTagCommit
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
		git tag "$major.$minor.$rev"
		git push
	}

}

CreateReleaseBranch