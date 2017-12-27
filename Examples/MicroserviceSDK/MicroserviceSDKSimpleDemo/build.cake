#addin "Cake.Docker"

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
var binaryDir = Directory("./source/DockerWebApp.Demo/bin");
var objectDir = Directory("./source/DockerWebApp.Demo/obj");
var projectFile = "./source/DockerWebApp.Demo/DockerWebApp.Demo.csproj";
var publishDir = "./publish";
var webProject = "./source/DockerWebApp.Demo";

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////


Task("Clean")
    .Does(() =>
    {
        CleanDirectory(binaryDir);
        CleanDirectory(objectDir);
		CleanDirectory(publishDir);

		if (FileExists("./images/hello-world-single/image.tar")){
			DeleteFile("./images/hello-world-single/image.tar");
		}
		if (FileExists("./images/hello-world-single/image.zip")){
			DeleteFile("./images/hello-world-single/image.zip");
		}		
		if (FileExists("./images/hello-world-multi/image.tar")){
			DeleteFile("./images/hello-world-multi/image.tar");
		}
		if (FileExists("./images/hello-world-multi/image.zip")){
			DeleteFile("./images/hello-world-multi/image.zip");
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
    var buildSettings = new DockerImageBuildSettings  { Tag = new[] {"webapp-demo:latest" }};
    DockerBuild(buildSettings, "./");

    var saveSettingsMulti = new DockerImageSaveSettings { Output = "./images/hello-world-multi/image.tar" };
    DockerSave(saveSettingsMulti,new[] {"webapp-demo:latest" });

	var filesMulti = new [] {
    "./images/hello-world-multi/image.tar",
    "./images/hello-world-multi/cumulocity.json" };

    Zip("./images/hello-world-multi", "image.zip", filesMulti);
	CopyFileToDirectory("image.zip", "./images/hello-world-multi");
	
	if (FileExists("./image.zip"))
	{
		DeleteFile("./image.zip");
	}
});

Task("Single-DockerImage")
.Does(() => {
    var saveSettingsSingle = new DockerImageSaveSettings { Output = "images/hello-world-single/image.tar" };
    DockerSave(saveSettingsSingle,new[] {"webapi-demo:latest" });

	var filesSingle = new [] {
    "./images/hello-world-single/image.tar",
    "./images/hello-world-single/cumulocity.json" };

    Zip("./images/hello-world-single", "image.zip", filesSingle);
	CopyFileToDirectory("image.zip", "./images/hello-world-single");
});

Task("Docker-Run")
.Does(() => {
	DockerRun("-p 8999:4700 webapp-demo:latest", "", "");
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