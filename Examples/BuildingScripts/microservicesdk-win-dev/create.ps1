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
$newNode = $xdoc.CreateElement("RestoreSources")
$newNode.InnerText = "`$(RestoreSources);../nugets;https://api.nuget.org/v3/index.json"
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

Invoke-WebRequest  http://resources.cumulocity.com/cssdk/releases/Cumulocity.AspNetCore.Authentication.Basic.9.1.0.nupkg -OutFile Cumulocity.AspNetCore.Authentication.Basic.9.1.0.nupkg
Invoke-WebRequest  http://resources.cumulocity.com/cssdk/releases/Cumulocity.SDK.Microservices.9.1.0.nupkg -OutFile Cumulocity.SDK.Microservices.9.1.0.nupkg

cd..
cd $WebApiProject 
$currentDir = Get-Location 
$nugetsFiles = Get-ChildItem $currentDir  -Filter *.nupkg 

foreach ($file in $nugetsFiles )
{
   $package = $file.Name -replace ".{12}$"
   dotnet add package "$package"
}

$csProgram="
  
using System.Net;
   public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
                WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection(""Logging""));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    var env = Environment.GetEnvironmentVariable(""ASPNETCORE_ENVIRONMENT"");
                    var port = Environment.GetEnvironmentVariable(""SERVER_PORT"");

                    int portNumber = 8080;
                    if (Int32.TryParse(port, out portNumber))
                    {
                        options.Listen(IPAddress.Parse(""0.0.0.0""), portNumber);
                    }
                    else
                    {
                        options.Listen(IPAddress.Parse(""0.0.0.0""), 1);
                    }
                })
                .Build();
    }
}"

$varOther = (get-content "Program.cs") | select-string -Pattern '{.*'  | Select-Object -ExpandProperty LineNumber
(get-content "Program.cs")   | select -First $varOther[0]  | Set-Content "Program.cs"
Add-Content -Path "Program.cs" -Value $csProgram

cd ../..
dotnet sln add "./src/$WebApiProject/$WebApiProject.csproj"

Pause
