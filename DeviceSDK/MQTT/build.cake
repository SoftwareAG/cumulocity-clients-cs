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
				CleanDirectory(outputDir);
			}
	});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => {
	    
	    var projects = GetFiles("./src/*.NetStandard/*.csproj");
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
	 
	  	var projects = GetFiles("./src/*.NetStandard/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCoreBuild(project.FullPath, buildSettings);
		}
    }
    else
    {
		// Use XBuild
		//DotNetCoreBuild(projJson, buildSettings);
	 	var projects = GetFiles("./src/*.NetStandard/*.csproj");
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
		var tests = GetFiles("./test/*.NetStandard/*.csproj");
		
		foreach(var test in tests)
		{
			var projectFolder = System.IO.Path.GetDirectoryName(test.FullPath);
			try
			{
			    Information("Solution Directory: {0}", solutionDir);
				DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings
				{
					ArgumentCustomization = args => args.Append("-l trx"),
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
		var tmpTestResultFiles = GetFiles("./*.NetStandard/*.trx");
		CopyFiles(tmpTestResultFiles, testResultDir);
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
		var projects = GetFiles("./src/*.NetStandard/*.csproj");
		foreach (var project in projects)
		{
		   DotNetCorePack(project.FullPath, packSettings);
		}
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
