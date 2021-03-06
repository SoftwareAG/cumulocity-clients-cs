# Device SDK for C sharp
------------------


*   [Introduction](#markdown-header-introduction)
    - [General prerequisites](#markdown-header-general-prerequisites)
    - [Source code](#markdown-header-source-code)
*   [MQTT implementation](#markdown-header-mqtt-implementation)
    - [Hello world example](#markdown-header-hello-world-mqtt-example)
    - [Developing MQTT clients](#markdown-header-developing-mqtt-clients)
*   [REST implementation](#markdown-header-rest-implementation)
    - [Hello world example](#markdown-header-hello-world-rest-example)
    - [Developing REST clients](#markdown-header-developing-rest-clients)

## Introduction

Cumulocity comes with elaborated support for developing clients in C#. You can use C#, for example, to:

* Interface Cumulocity with open, C#-enabled devices through a device-side agent. Today, many embedded Linux devices such as the Raspberry Pi, support C#.
* Interface Cumulocity with closed devices speaking an existing, Internet-enabled protocol through a server-side agent.

### General prerequisites

To use the C# client libraries, you need to have at least version 2.2 of the [.NET Core](https://dotnet.microsoft.com/download/dotnet-core/2.2) install on your locale machine. To verify the version of your .NET Core Software Development Kit, type:

```bash
$ dotnet --version
```

The output needs to show a version number later than "2.2.100" for the basic examples.

You also need to have a valid tenant, user and password in order to access Cumulocity. Refer to [Tenant ID and tenant domain](https://cumulocity.com/guides/reference/tenants/#tenant-id-and-domain) in the Reference guide to learn the difference between tenant ID and tenant domain.

### Source code

The source code of the MQTT and REST examples can be found in the [cumulocity-sdk-cs repository](https://bitbucket.org/m2m/cumulocity-sdk-cs/src/master/).

## MQTT implementation

Cumulocity supports MQTT both via TCP and WebSockets. As URL you use mqtt.cumulocity.com. The Cumulocity MQTT implementation uses SmartREST as a payload. SmartREST is a CSV-like message protocol that uses templates on the server side to create data in Cumulocity. Refer to [Device integration using MQTT](https://cumulocity.com/guides/device-sdk/mqtt/) for more details.

### Hello world MQTT example

The easiest starting point is the [Hello world example](MQTT/hello-world.md) following the steps presented in the example.

### Developing MQTT clients

It is possible to develop clients using the Cumulocity [MQTT static templates](MQTT/cs-static-templates.md) for C#.

## REST implementation

These are some references for getting started with the basic technologies underlying the SDK:

*   The client libraries use the Cumulocity REST interfaces as underlying communication protocol as described in [Device integration using REST](https://cumulocity.com/guides/device-sdk/rest/).
*   All examples and libraries are open source and can be found at the [Bitbucket M2M repositories](https://bitbucket.org/m2m).

### Hello world REST example

The easiest starting point is the [Hello world example](REST/hello-world.md). Note that you can develop with any IDE and any build tool that you prefer, but the examples focus on .NET Core SDK and Visual Studio.

### Developing REST clients

This section gives an overview on how to access Cumulocity from C# clients, starting from connecting to the Cumulocity platform over accessing data to remote control of devices. It also shows how to extend the Cumulocity domain model from C# for new devices and other business objects. Finally, [Developing REST clients](REST/developing-cs-clients.md) describes how to configure the logging service in order to control the level of diagnostic messages generated by the client.
