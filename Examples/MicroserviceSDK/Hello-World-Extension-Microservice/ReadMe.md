### Overview

This example provides a step-by-step guide to develop a simple microservice in Cumulocity. It uses Cake (C# Make), which is a cross-platform build automation system.

To start building .NET apps, you just need to download and install the [.NET SDK](https://www.microsoft.com/net/download). Follow the instructions on the download page for the last stable release or alternatively you can also try using 3.1.

If you use Linux, visit the [MonoDevelop website](http://www.monodevelop.com/) for download packages and more details about our cross-platform IDE. Follow the instructions on the download page for the last stable release or alternatively you can also try using 6.8 or higher version of mono [IDE](http://www.mono-project.com/download/#download-lin). Note, that Mono-devel is required to compile code.

The initial script was used to create a demo, which makes it easier to create an example microservice project with dependency management and then deploy it on the server. The script attempts to download the package from the sources listed in the project file, and next a reference is added to the appropriate project file. In addition, the script creates the appropriate Dockerfile to take into account the naming of projects. Next it will create a Docker image based on a Dockerfile.

The application created in this way uses the ASP.NET Web API framework to create a web API. The API runs on an isolated web server called Kestrel and as a foreground job, which makes it work really well with Docker.


### Building and deploying Hello World on Windows

Refer to the [online documentation|https://cumulocity.com/guides/microservice-sdk/cs/] and follow the step-by-step tutorial to develop a simple microservice in Cumulocity IoT. The online documentation also contains more details about building microservices with the SDK for C#.

### Extending hello-world microservice to create a smart thermometer microservice

This microservice in this folder is an extension of the hello world microservice mentioned in [online documentation|https://cumulocity.com/guides/microservice-sdk/cs/] .
This scenario has smart thermometers that send readings to Cumulocity IoT. This example has an extra Controller class called `ThermostatController` which has 3 new endpoints with the following Implementation -

- Endpoint - 1
Implement the endpoint <microservice-name>/<controller-name>/thermometers 
Action: Register a new thermometer    
Method: POST    
Payload:
  {
    "nameID" : "<thermometer-name>",
    "TemperatureC" : int 
  }

The endpoint internally calls /inventory/managedObjects POST request with the below mentioned payload to create a thermometer in the platform -
{
  "name": "<thermometer-name>"
  "c8y_IsDevice": {}, 
  "c8y_IsThermometer": {},
  "c8y_SupportedMeasurements": [
    "c8y_TemperatureMeasurement"
  ]
}

The custom fragment `c8y_IsThermometer` is used to identify the devices as thermometer.

- Endpoint - 2
Implement the endpoint <microservice-name>/<controller-name>/thermometers
Action: Retrieve all registered thermometers    
Method: GET     
internally the microservice calls /inventory/managedObjects?fragmentType=c8y_IsThermometer&fragmentType=c8y_SupportedMeasurements

- Endpoint - 3
Implement the endpoint <microservice-name>/thermometers/{id}/temperatures/ 
Method: POST 
Payload:
{
    "alarmText" : <Alarm-Text>
}  

Action: When invoked with a POST request, internally the microservice will invoke the already implemented (Hello world) endpoint <microservice-name>/weatherforecast to take the latest entry of weather forecasts and if any forecast is greater than 25º then post the temperatureC value to Cumulocity IoT under the specified device (by id). Below are the specification of the alarm to be raised -
Severity: WARNING 
Type: c8y_hightemperature_alarm 
Text: As fetched from the payload body.


