#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.DocFx&version=0.7.0"
#tool "nuget:https://api.nuget.org/v3/index.json?package=docfx.console&version=2.38.1"
#addin "Cake.MiniCover"


//////////////////////////////////////////////////////////////////////
/// ARGUMENTS
//////////////////////////////////////////////////////////////////////
 
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
  
//////////////////////////////////////////////////////////////////////
///    Build Variables
/////////////////////////////////////////////////////////////////////
var binDir = "";       //Destination Binary File Directory name i.e. bin
var solutionFile = ""; // Solution file if needed
var outputDir = Directory("./publish") + Directory(configuration);  // The output directory the build artefacts saved too

var destinationIp = "";
var destinationDirectory = "";
var username = "";

var testFailed = false;
var solutionDir = System.IO.Directory.GetCurrentDirectory();
var testResultDir = System.IO.Path.Combine(solutionDir, "test-results");
var artifactDir = "./artifacts";
var docfxDir = "./docs/_site";


var local = BuildSystem.IsLocalBuild;
string version = null;
string semanticVersion = null;
string prerelease = null;
string isTagged = null;
var isReleaseBranch = false;

var isCreatedRelease = false;
 
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
 
Information("Solution Directory: {0}", solutionDir);
Information("Test Results Directory: {0}", testResultDir);

Task("PrepareDirectories")
	.Does(() =>
	{
		EnsureDirectoryExists(testResultDir);
		EnsureDirectoryExists(artifactDir);
	});
	
Task("Clean")
    .IsDependentOn("PrepareDirectories")
    .Does(() =>
	{		
		if (DirectoryExists(outputDir))
			{
				//DeleteDirectory(outputDir, recursive:true);
				CleanDirectory(outputDir);
			}
		if (DirectoryExists(docfxDir))
			{
				CleanDirectory(docfxDir);
			}
		if (DirectoryExists(testResultDir))
			{
				CleanDirectory(testResultDir);
			}
			
		var tests = GetFiles("./test/**/*.csproj");
		
		foreach(var test in tests)
		{
			var projectFolder = System.IO.Path.GetDirectoryName(test.FullPath);
			Information("CleanDirectory {0}", System.IO.Path.Combine(projectFolder, "TestResults"));
			CleanDirectory(System.IO.Path.Combine(projectFolder, "TestResults"));
		}
	});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => {
	    
	    var projects = GetFiles("./src/**/*.csproj");
		foreach (var project in projects)
		{
		   Information(project.FullPath);
		   DotNetCoreRestore(project.FullPath);
		}
});
 
Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
	
	var buildSettings = new DotNetCoreBuildSettings
     {
         Configuration = configuration
     };
	 
    if(IsRunningOnWindows())
    {
	 
	  	var projects = GetFiles("./src/**/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCoreBuild(project.FullPath, buildSettings);
		}
    }
    else
    {
		// Use XBuild
		//DotNetCoreBuild(projJson, buildSettings);
	 	var projects = GetFiles("./src/**/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCoreBuild(project.FullPath, buildSettings);
		}
    }
});
	
 Task("Test")
	.IsDependentOn("Clean")
	.IsDependentOn("Build")
	.ContinueOnError()
	.Does(() =>
	{
		var tests = GetFiles("./test/**/*.csproj");
		
		foreach(var test in tests)
		{
			var projectFolder = System.IO.Path.GetDirectoryName(test.FullPath);
			var projectName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(test.FullPath));
			DotNetCoreRestore(test.FullPath);
					
			try
			{
			    Information("Solution Directory: {0}", solutionDir);
				
				var resultFile = "ResultTest_"+projectName+".xml";
				
				DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings
				{
					ArgumentCustomization = args => args.Append("-l \"trx;LogFileName="+resultFile+"\""),
					WorkingDirectory = projectFolder
				});
			}
			catch(Exception e)
			{
				testFailed = true;
				Error(e.Message.ToString());
			}
		}

		var tmpTestResultFiles = GetFiles("./**/ResultTest_*");
		CopyFiles(tmpTestResultFiles, testResultDir);		
		var resultTests = GetFiles("./test-results/ResultTest_*");
		
		foreach(var resultFile in resultTests)
		{
			Information("The file name in a dict: {0} {1}",resultFile,System.IO.Path.GetFileName(resultFile.FullPath));
			XmlTransform("./tools/MsUnit.xslt", resultFile , testResultDir +"/JUnit_" + System.IO.Path.GetFileName(resultFile.FullPath));	 
		}
	});

Task("Package")   
	.IsDependentOn("Test") 
	//.IsDependentOn("Build")
	//.IsDependentOn("Test")
	//.IsDependentOn("Docs")
	.ContinueOnError()  	
	.Does(() => { 
	        
			var buildSettings = new DotNetCoreMSBuildSettings();
			//buildSettings.SetVersion(version);
	  
        	var packSettings = new DotNetCorePackSettings  
       		{             OutputDirectory = outputDir,
						  NoBuild = true,
                          Configuration = configuration,
						  MSBuildSettings = buildSettings
       		};           
        //DotNetCorePack(projJson, packSettings);
		
		var projects = GetFiles("./src/**/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCorePack(project.FullPath, packSettings);
		}
});

Task("Docs").Does(() => DocFxBuild("./docs/docfx.json"));

Task("CreateRelease")
.IsDependentOn("Test")
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


//Task("Deploy")
//    .IsDependentOn("Package")
//    .Does(() =>
//    {
//			var files = GetFiles("./publish/Release/*");
//			var fileArray = files.Select(m => m.ToString()).ToArray();		
//	        var destination = destinationIp + ":" + destinationDirectory;
//			var projects = GetFiles("./publish/Release/*.nupkg");
//			foreach (var project in projects)
//			{
//		        	    Information(project.GetFilename());
//			   			Pscp("./publish/Release/" + project.GetFilename(), destination, new PscpSettings
//							{
//								SshVersion=SshVersion.V2,
//								User=username
//								//,KeyFileForUserAuthentication="~/.ssh/key.ppk"
//							}
//						);
//			}			        
//    });
Task("Coverage")
    .IsDependentOn("Build")
    .Does(() => 
{
    MiniCover(tool =>
        {
            foreach(var project in GetFiles("./test/**/*.csproj"))
            {
                tool.DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings()
                {
                    // Required to keep instrumentation added by MiniCover
                    NoBuild = true,
                    Configuration = "Debug"
                });
            }
        },
        new MiniCoverSettings()
            .WithAssembliesMatching("./test/**/*.dll")
            .WithSourcesMatching("./src/**/*.cs")
            .GenerateReport(ReportType.CONSOLE | ReportType.XML)
    );
});
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
 
Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
 
RunTarget(target);