function ReadCommitCountInBranch
(
    [string] $branch
)
{

	$hglog = (& hg log -r "branch('$branch')" --template "{node}\n"  )
	
	if ($hglog -eq $null) 
	{
		return 0;
	}else
	{	
		$commitCountInBranch = (& hg log -r "branch('$branch')" --template "{node}\n"  | Measure-Object -line ).Lines
		return $commitCountInBranch ;
	}
}
function ReadBranchName
{
    $branchName = (& hg branch).Trim()
    return $branchName
}
function ReadCommitCount
{
    $count = (& hg id --num --rev tip).Trim()
    return $count
}

function ReadIsLastTagCommit
(
    [string] $branch
)
{
    Try
    {
		$lasttag = (& hg log -r "branch('$branch') and last(tag('re:^r\d*'))" --template "{tags}\n").Trim()
				
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
		hg up $LastTagCommit
	    hg flow release start $LastTagCommit
		hg push
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
		hg tag "$major.$minor.$rev"
		hg push
	}

}

CreateReleaseBranch