#addin "Cake.Putty"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
 
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
  
//////////////////////////////////////////////////////////////////////
///    Build Variables
/////////////////////////////////////////////////////////////////////
var binDir = "";       //Destination Binary File Directory name i.e. bin
var solutionFile = ""; // Solution file if needed
var outputDir = Directory("./publish") + Directory(configuration);  // The output directory the build artefacts saved too

var destinationIp = "52.174.102.182";
var destinationDirectory = "/home/pnow/upload";
var username = "pnow";

var testFailed = false;
var solutionDir = System.IO.Directory.GetCurrentDirectory();
var testResultDir = System.IO.Path.Combine(solutionDir, "test-results");
var artifactDir = "./artifacts";
 
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
		//, OutputDirectory = outputDir
     };
	 
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      // MSBuild(solutionFile , settings => settings.SetConfiguration(configuration));
	 
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
			try
			{
			    Information("Solution Directory: {0}", solutionDir);
				DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings
				{
					ArgumentCustomization = args => args.Append("-l \"trx;LogFileName=Result.xml\""),
					WorkingDirectory = projectFolder
				});
			}
			catch(Exception e)
			{
				testFailed = true;
				Error(e.Message.ToString());
			}
		}

		// Copy test result files.
		var tmpTestResultFiles = GetFiles("./**/*.trx");
		CopyFiles(tmpTestResultFiles, testResultDir);
		XmlTransform("./tools/MsUnit2.xslt", testResultDir +"/Result.xml", testResultDir +"/JUnit.Result.xml");	
	});

Task("Package")    
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.ContinueOnError()  	
	.Does(() => { 
        	var packSettings = new DotNetCorePackSettings  
       		{             OutputDirectory = outputDir,
						  NoBuild = true,
                          Configuration = configuration
       		};           
        //DotNetCorePack(projJson, packSettings);
		
		var projects = GetFiles("./src/**/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCorePack(project.FullPath, packSettings);
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
 
Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
 
RunTarget(target);
