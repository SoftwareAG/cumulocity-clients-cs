//#addin nuget:https://www.nuget.org/api/v2/?package=cake.docker
//#addin Cake.Hg

using Path = System.IO.Path;
using IO = System.IO;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
//////////////////////////////////////////////////////////////////////

var projectName = "[ProjectName]";
var binaryDir = Directory("./src/"+ projectName +"/bin");
var objectDir = Directory("./src/"+ projectName +"/obj");
var projectFile = "./src/"+ projectName +"/"+ projectName +".csproj";
var publishDir = "./publish";
var webProject = "./src/"+ projectName +"";
var imageName = ""+ projectName +":latest"; 
    imageName = imageName.ToLowerInvariant();
	
var local = BuildSystem.IsLocalBuild;
string version = null;
string semanticVersion = null;
string prerelease = null;
string isTagged = null;
var isReleaseBranch = false;

var isCreatedRelease = false;

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////


Task("Clean")
    .Does(() =>
    {
	
    });


Task("CreateRelease")
.Does(() => {
		Information("CreateRelease");
		
		Information(@"BuildNumber: {0}",BuildSystem.Jenkins.Environment.Build.BuildNumber);
	
		var settings = new ProcessSettings
		{
		   Arguments = new ProcessArgumentBuilder().Append("hgversion.ps1 -local false")
		};
		StartProcess("pwsh", settings);
		
		if (FileExists("./version.props"))
		{
			
			string[] lines = System.IO.File.ReadAllLines("./version.props");
			
			foreach (string line in lines)
			{
					if (line.StartsWith("version"))
					{
						version = line.Substring("version=".Length).Trim();
					}
					else if (line.StartsWith("semanticVersion"))
					{
						semanticVersion = line.Substring("semanticVersion=".Length).Trim();
					}
					else if (line.StartsWith("prerelease"))
					{
						prerelease = line.Substring("prerelease=".Length).Trim();
					}
					else if (line.StartsWith("istagged"))
					{
						isTagged = line.Substring("istagged=".Length).Trim();
					}
			}
			
			var newVersion = Convert.ToInt32(version.Replace(".",""));
			var oldVersion = Convert.ToInt32(isTagged.Replace(".",""));
			
			if(newVersion <= oldVersion)
			{
			    throw new Exception("The version of build is incorrect.");  
			}else{
			
			   			
				var settingsCreateRelease = new ProcessSettings
				{
				   Arguments = new ProcessArgumentBuilder().Append("createrelease.ps1 -version " + version)
				};
				isCreatedRelease = true;
				Information("Settings release:" + version);								
				Information("isCreatedRelease:" + isCreatedRelease);	
				StartProcess("pwsh", settingsCreateRelease);
			}
			
		}else{
			throw new Exception("Version.props not found");  
		}
	
   
});

Task("PublishRelease")
//.WithCriteria(isCreatedRelease == true)
//.IsDependentOn("CreateRelease")
.Does(() => {
	Information("PublishRelease");
	version = "9.1.0";
	Zip("./microservicesdk-win-dev", "microservicesdk-win-dev-"+ version+".zip");
	Zip("./microservicesdk-lin-dev", "microservicesdk-lin-dev-"+ version+".zip");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("PublishRelease");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);