#!/usr/bin/env bash
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Stop script on NZEC
set -e
# Stop script if unbound variable found (use ${var:-} if intentional)
set -u

# Use in the the functions: eval $invocation
invocation='say_verbose "Calling: ${yellow:-}${FUNCNAME[0]} ${green:-}$*${normal:-}"'

# standard output may be used as a return value in the functions
# we need a way to write text on the screen in the functions so that
# it won't interfere with the return value.
# Exposing stream 3 as a pipe to standard output of the script itself
exec 3>&1

ARG1=${1:-project}
ARG2=${2:-api}

if [ -n "$ARG1" ] && [ -n "$ARG2" ]; then
  if [ $ARG1 = "project" ] && [ $ARG2 = "api" ]; then
	
		echo "Enter the solution name:"
		read projectName
		echo "Enter the name of a web API project:"
		read webApiProject
   else
	   projectName=$ARG1
	   webApiProject=$ARG2
   fi
fi

##########
cakeBuildOutput="build.cake"
dockerfileOutput="Dockerfile"
deployOutput="deploy.sh"
##########

mkdir $projectName

echo "$projectName/$cakeBuildOutput"
mv "$cakeBuildOutput" "$projectName/$cakeBuildOutput"
mv "$dockerfileOutput" "$projectName/$dockerfileOutput"
mv "$deployOutput" "$projectName/$deployOutput"

cd $projectName

curl -Lsfo build.sh https://cakebuild.net/download/bootstrapper/linux
chmod +x build.sh


sed -i 's/\[ProjectName\]/'"$webApiProject"'/g' $cakeBuildOutput
sed -i 's/DockerApp.dll/'"$webApiProject.dll"'/g' $dockerfileOutput

dotnet new sln --name "$projectName" 
mkdir publish
mkdir -p images/single
mkdir -p images/multi
mkdir src
destdir="images/multi/cumulocity.json"
echo $destdir
touch "$destdir"

cumulocityJson='{ "apiVersion":"1",
                "version":"1.0.0" }'

cumulocityJson='{
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

#echo "$cumulocityJson" >> $destdir

if [ -f "$destdir" ]
then
	echo "$cumulocityJson" >> $destdir
fi

cd src
dotnet new webapi --name "$webApiProject" --output "$webApiProject"

mkdir nugets
cd nugets



wget http://resources.cumulocity.com/cssdk/releases/Cumulocity.AspNetCore.Authentication.Basic.1.0.0.nupkg
wget http://resources.cumulocity.com/cssdk/releases/Cumulocity.SDK.Microservices.1.0.0.nupkg

cd ..
cd $webApiProject
restoreSources="\$(RestoreSources);../nugets;https://api.nuget.org/v3/index.json"

sed -i 's,<\/PropertyGroup>,<PublishWithAspNetCoreTargetManifest>false</PublishWithAspNetCoreTargetManifest><RestoreSources>'"$restoreSources"'<\/RestoreSources>\n<\/PropertyGroup>,g' "$webApiProject.csproj"

for f in ../nugets/*.nupkg;do
pkg=$(basename $f);
pkg=$(sed 's/.\{12\}$//' <<< "$pkg");
dotnet add package $pkg;	
done;

echo "Packages were added";

csProgram="
{   
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
                    logging.AddConfiguration(hostingContext.Configuration.GetSection(\"Logging\"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    var env = Environment.GetEnvironmentVariable(\"ASPNETCORE_ENVIRONMENT\");
                    var port = Environment.GetEnvironmentVariable(\"SERVER_PORT\");

                    int portNumber = 8080;
                    if (Int32.TryParse(port, out portNumber))
                    {
                        options.Listen(IPAddress.Parse(\"0.0.0.0\"), portNumber);
                    }
                    else
                    {
                        options.Listen(IPAddress.Parse(\"0.0.0.0\"), 1);
                    }
                })
                .Build();
    }
}"
sed -i '/{/Q' "Program.cs"

echo "$csProgram" >> "Program.cs"

cd ../..


