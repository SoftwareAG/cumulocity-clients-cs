#addin nuget:https://www.nuget.org/api/v2/?package=cake.docker&version=0.10.0

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

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////


Task("Clean")
    .Does(() =>
    {
        CleanDirectory(binaryDir);
        CleanDirectory(objectDir);
		CleanDirectory(publishDir);

		if (FileExists("./images/single/image.tar")){
			DeleteFile("./images/single/image.tar");
		}
		if (FileExists("./images/single/image.zip")){
			DeleteFile("./images/single/image.zip");
		}		
		if (FileExists("./images/multi/image.tar")){
			DeleteFile("./images/multi/image.tar");
		}
		if (FileExists("./images/multi/image.zip")){
			DeleteFile("./images/multi/image.zip");
		}
		if (FileExists("./image.zip")){
			DeleteFile("./image.zip");
		}
    });

Task("Build")
  .IsDependentOn("Clean")
  .Does(()=>{

	DotNetCoreRestore(projectFile);	
	DotNetCoreBuild(projectFile, new DotNetCoreBuildSettings
    {
        Configuration = configuration
    });
    //MSBuild(projectFile);
  });

Task("DotnetPublish")
    .IsDependentOn("Clean")
    .Does(() =>
	{
		DotNetCorePublish(webProject, new DotNetCorePublishSettings
		{
			Configuration = configuration,
			OutputDirectory = Path.Combine(publishDir, "Web")
		});
	});

Task("Docker-Build")
.IsDependentOn("DotnetPublish")
.Does(() => {
    var buildSettings = new DockerImageBuildSettings  { Tag = new[] {imageName}};
    DockerBuild(buildSettings, "./");

    var saveSettingsMulti = new DockerImageSaveSettings { Output = "./images/multi/image.tar" };
    DockerSave(saveSettingsMulti,new[] {imageName});

    var filesMulti = new [] {
    "./images/multi/image.tar",
    "./images/multi/cumulocity.json" };

    Zip("./images/multi", "image.zip", filesMulti);
	CopyFileToDirectory("image.zip", "./images/multi");
	
	if (FileExists("./image.zip"))
	{
		DeleteFile("./image.zip");
	}
});

Task("Single-DockerImage")
.Does(() => {
    var saveSettingsSingle = new DockerImageSaveSettings { Output = "images/single/image.tar" };
    DockerSave(saveSettingsSingle,new[] {imageName});

    var filesSingle = new [] {
    "./images/single/image.tar",
    "./images/single/cumulocity.json" };

    Zip("./images/single", "image.zip", filesSingle);
	CopyFileToDirectory("image.zip", "./images/single");
});

Task("Docker-Run")
.Does(() => {
	DockerRun("-p 8999:4700 "+ imageName, "", "");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("Docker-Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);