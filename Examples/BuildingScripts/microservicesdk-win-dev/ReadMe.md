### Overview

This example provides a step-by-step guide to develop a simple microservice in Cumulocity. It uses Cake (C# Make), which is a cross-platform build automation system.

To start building .NET apps, you just need to download and install the [.NET SDK](https://www.microsoft.com/net/download). Follow the instructions on the download page for the last stable release or alternatively you can also try using 3.1.

If you use Linux, visit the [MonoDevelop website](http://www.monodevelop.com/) for download packages and more details about our cross-platform IDE. Follow the instructions on the download page for the last stable release or alternatively you can also try using 6.8 or higher version of mono [IDE](http://www.mono-project.com/download/#download-lin). Note, that Mono-devel is required to compile code.

The initial script was used to create a demo, which makes it easier to create an example microservice project with dependency management and then deploy it on the server. The script attempts to download the package from the sources listed in the project file, and next a reference is added to the appropriate project file. In addition, the script creates the appropriate Dockerfile to take into account the naming of projects. Next it will create a Docker image based on a Dockerfile.

The application created in this way uses the ASP.NET Web API framework to create a web API. The API runs on an isolated web server called Kestrel and as a foreground job, which makes it work really well with Docker.


### Building and deploying Hello World on Windows

Refer to the [online documentation|https://cumulocity.com/guides/microservice-sdk/cs/] and follow the step-by-step tutorial to develop a simple microservice in Cumulocity IoT. The online documentation also contains more details about building microservices with the SDK for C#.

