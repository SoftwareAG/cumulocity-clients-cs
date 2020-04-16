Param(
[string]$ProjectName = "Project",
[string]$WebApiProject = "WebApi"
)

If( ($ProjectName -And !$WebApiProject) -Or (!$ProjectName -And $WebApiProject) )
{     
     Write-Host "Break Out! The script needs two not null parameters."
     Break
}
 
If( (!$ProjectName -And !$WebApiProject) -Or ($ProjectName -eq "Project" -And $WebApiProject -eq "WebApi") )
{
	$ProjectName = Read-Host -Prompt 'Enter the solution name:'
	$WebApiProject = Read-Host -Prompt 'Enter the name of a web API project:'
}
 
If(!$ProjectName -And !$WebApiProject)
{
	$ProjectName = Read-Host -Prompt 'Enter the solution name:'
	$WebApiProject = Read-Host -Prompt 'Enter the name of a web API project:'
}
 

IF ([string]::IsNullOrWhitespace($ProjectName )){
	Write-Host "The solution name is empty.";
	exit;
} 
IF ([string]::IsNullOrWhitespace($WebApiProject)){ 
	Write-Host "The name of web api project is empty."
	exit;
} 
#####################
#######CREATE########
#####################
function Select-Value {
  param
  (
    [Parameter(Mandatory=$true, ValueFromPipeline=$true, HelpMessage="Data to process")]
    $InputObject
  )
  process {
     $InputObject.Value
  }
}
function Get-IniFile {
    param(
        [parameter(Mandatory = $true)] [string] $filePath
    )

    $anonymous = "NoSection"

    $ini = @{}
    switch -regex -file $filePath {
        "^\[(.+)\]$"   {
            $section = $matches[1]
            $ini[$section] = @{}
            $CommentCount = 0
        }

        "^(;.*)$"   {
            if (!($section)) {
                $section = $anonymous
                $ini[$section] = @{}
            }
            $value = $matches[1]
            $CommentCount = $CommentCount + 1
            $name = "Comment" + $CommentCount
            $ini[$section][$name] = $value
        }

        "(.+?)\s*=\s*(.*)"  {
            if (!($section)) {
                $section = $anonymous
                $ini[$section] = @{}
            }
            $name,$value = $matches[1..2]
            $ini[$section][$name] = $value
        }
    }

    return $ini
}

$file = "settings.ini"
$isLocalFile = $true
$settingsFile = (Get-Childitem  -Include *settings.ini* -File -Recurse )  | % { $_.FullName }

if (($settingsFile) -and (Test-Path $settingsFile)) {
    $isLocalFile = $true;
}
else{
	
    $isLocalFile = $false
    $appdata = Get-Childitem env:APPDATA | Select-Value
    $file = "$appdata\c8y\$file"
		
}
if ( (-not(Test-Path $file)) -and (-not(Test-Path .\$file))){ 
  #throw [IO.FileNotFoundException] "$file not found."
}else{
	
	if(-not($isLocalFile)) {
		$settingsIni = Get-IniFile $file
	}
	else
	{
		$settingsIni = Get-IniFile .\$file
	}
		
	$username = $settingsIni.deploy.username
	$password = $settingsIni.deploy.password
	$url = $settingsIni.deploy.url
	$appname = $settingsIni.deploy.appname
        $base64AuthInfo =  [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

	Write-Host "https://$url/application/applications"

	$Result = Invoke-RestMethod -Uri "https://$url/application/applicationsByName/$appname" `
							-Headers @{Authorization = ("Basic {0}" -f $base64AuthInfo)} `
							-ErrorVariable RestError -ErrorAction "SilentlyContinue"
           if ($RestError) {
        $HttpStatusCode = $RestError.ErrorRecord.Exception.Response.StatusCode.value__
        $HttpStatusDescription = $RestError.ErrorRecord.Exception.Response.StatusDescription

        Write-Output "[ERROR] Error while connecting to platform"
        Write-Output "Http Status Code: $($HttpStatusCode) `nHttp Status Description: $($HttpStatusDescription)"
        Exit
    }
    else {

        if( -not($Result.applications.id))
            {
                       $json = '{
			"name": "' + (echo $appname) + '",
			"type": "MICROSERVICE",
			"key": "' + (echo $appname) + '-key"
	                }'

	        $headers = @{
		        Authorization=("Basic {0}" -f $base64AuthInfo)
	        	Accept ="application/json"
	        }
	        Invoke-WebRequest -Uri "https://$url/application/applications" -Headers $headers -Method Post -Body $json -ContentType "application/json"
            }
	}
}
###########################
######Main Project#########
###########################

mkdir "$ProjectName" 
$currentDir = Get-Location
$CakeBuildOutput = "$currentDir/build.cake" 
$DockerfileOutput = "$currentDir/Dockerfile"
$DeployfileOutput = "$currentDir/deploy.ps1"

Move-Item "$CakeBuildOutput" "$currentDir/$projectName/build.cake"
Move-Item "$DockerfileOutput" "$currentDir/$projectName/Dockerfile"
Move-Item "$DeployfileOutput" "$currentDir/$projectName/deploy.ps1"

(Get-Content "$currentDir\$projectName\build.cake").replace('[ProjectName]', "$WebApiProject") | Set-Content "$currentDir\$projectName\build.cake"
(Get-Content "$currentDir\$projectName\Dockerfile").replace('DockerApp.dll', "$WebApiProject.dll") | Set-Content "$currentDir\$projectName\Dockerfile"

cd "$ProjectName" 
$currentDir = Get-Location 

Invoke-WebRequest https://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1

dotnet new sln --name "$ProjectName" 


mkdir tools
cd tools
$packagescakedir='packages.config'
New-Item "$packagescakedir" -type file
$cakePackages='<?xml version="1.0" encoding="utf-8"?>
<packages>
    <package id="Cake" version="0.37.0" />
</packages>'
Add-Content $packagescakedir $cakePackages
cd ..
#####################
######Source#########
#####################
mkdir publish
mkdir -p images/single
mkdir -p images/multi
mkdir src

$CumulocityJson = '{
  "apiVersion": "1",
  "version": "1.0.0",
  "provider": {
    "name": "Cumulocity GmbH"
  },
  "contextPath": "hello",
  "isolation": "MULTI_TENANT",
  "resources": {
    "cpu": "2000m",
    "memory": "2Gi"
  }
}'
New-Item images\multi\cumulocity.json -type file
Add-Content images\multi\cumulocity.json $CumulocityJson 

cd src
#####################
######WebApi#########
#####################
dotnet new webapi --name "$WebApiProject" --output "$WebApiProject"
$currentDir = Get-Location 
$filePath = "$currentDir/$WebApiProject/$WebApiProject.csproj"

[Xml]$xdoc = Get-Content -Path $filePath -Raw
#RestoreSources
$newNode = $xdoc.CreateElement("RestoreSources")
$newNode.InnerText = "`$(RestoreSources);../nugets;https://api.nuget.org/v3/index.json"
$xdoc.SelectSingleNode("//PropertyGroup[1]").appendChild($newNode)
#PublishWithAspNetCoreTargetManifest
$newNode = $xdoc.CreateElement("PublishWithAspNetCoreTargetManifest")
$newNode.InnerText = "false"
$xdoc.SelectSingleNode("//PropertyGroup[1]").appendChild($newNode)

$xdoc.Save($filePath)
#####################
######Nugets#########
#####################
mkdir nugets
cd nugets 
$currentDir = Get-Location 
$start_time = Get-Date

##FTP
 $target = "$currentDir/"

Invoke-WebRequest  http://resources.cumulocity.com/cssdk/releases/Cumulocity.AspNetCore.Authentication.Basic.9.20.0.nupkg -OutFile Cumulocity.AspNetCore.Authentication.Basic.9.20.0.nupkg
Invoke-WebRequest  http://resources.cumulocity.com/cssdk/releases/Cumulocity.SDK.Microservices.9.20.0.nupkg -OutFile Cumulocity.SDK.Microservices.9.20.0.nupkg

$nugetsFiles = Get-ChildItem $currentDir  -Filter *.nupkg  

cd.. 
cd $WebApiProject  

$currentDir = Get-Location  

foreach ($file in $nugetsFiles ) 
{ 
   $package = $file.Name 
   if ($package -like '*Authentication.Basic*') { 
	$package =($package.Split(".",5) | Select -Index 0,1,2,3) -join "."  
   }
   elseif ($package -like '*Cumulocity.SDK.Microservices*'){
	$package =($package.Split(".",4) | Select -Index 0,1,2) -join "."  
   }  
   Write-Host $package
   dotnet add package "$package" 
} 

$csStartupFile ="Startup.cs"

$csStartup="

	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using Newtonsoft.Json.Serialization;
    using Cumulocity.SDK.Microservices.BasicAuthentication;
    using Cumulocity.SDK.Microservices.Configure;
    using Cumulocity.SDK.Microservices.Services;
    using Cumulocity.SDK.Microservices.Settings;
    using Cumulocity.SDK.Microservices.Utils;
    using Microsoft.AspNetCore.Builder;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddCumulocityAuthentication(Configuration);
			services.AddPlatform(Configuration);
			services.AddSingleton<IApplicationService, ApplicationService>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//MVC
			services.AddMvc().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
			services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseAuthentication();
			app.UseBasicAuthentication();
			app.UseMvcWithDefaultRoute();
		}
	}
}"

$varStartup = (get-content  -Path  $csStartupFile) | select-string -Pattern 'namespace. *'  | Select-Object -ExpandProperty LineNumber
$toLineNo = $varStartup[0] - 2;
(Get-Content $csStartupFile | Select-Object -Skip $toLineNo) | Set-Content $csStartupFile

$varStartup = (get-content $csStartupFile) | select-string -Pattern '{.*'  | Select-Object -ExpandProperty LineNumber
(get-content $csStartupFile)   | select -First $varStartup[0]  | Set-Content $csStartupFile 
Add-Content -Path $csStartupFile -Value $csStartup

$csProgram="
  
using System.Net;
using Cumulocity.SDK.Microservices.Configure;
   public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					var port = Environment.GetEnvironmentVariable(""SERVER_PORT"");
					options.Listen(IPAddress.Parse(""0.0.0.0""), Int32.TryParse(port, out var portNumber) ? portNumber : 8080);
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));
					logging.AddConsole().SetMinimumLevel(LogLevel.Information);
				})
				.UseStartup<Startup>()
				.Build();
    }
}"

$varOther = (get-content "Program.cs") | select-string -Pattern '{.*'  | Select-Object -ExpandProperty LineNumber
(get-content "Program.cs")   | select -First $varOther[0]  | Set-Content "Program.cs"
Add-Content -Path "Program.cs" -Value $csProgram

cd ../..
dotnet sln add "./src/$WebApiProject/$WebApiProject.csproj"

Pause
