<h1>Device SDK for C#</h1><br>

**Contents**

*   [Introduction](#markdown-header-introduction)
*   []()


## Introduction

Cumulocity comes with elaborated support for developing clients in C#. You can use C#, for example, to:

* Interface Cumulocity with open, C#-enabled devices through a device-side agent. Today, many embedded Linux devices such as the Raspberry Pi, support C#.
* Interface Cumulocity with closed devices speaking an existing, Internet-enabled protocol through a server-side agent.

The easiest starting point is the [Hello world example](/guides/device-sdk/device-sdk-cs/#hello-world-basic). Note, that you can develop with any IDE and any build tool that you prefer, but the examples focus on .NET Core SDK and Visual Studio.

Finally, here are some references for getting started with the basic technologies underlying the SDK:

*   The client libraries use the Cumulocity REST interfaces as underlying communication protocol as described in [Device integration using REST](https://cumulocity.com/guides/device-sdk/rest/).
*   All examples and libraries are open source and can be found at the [Bitbucket M2M repositories](https://bitbucket.org/m2m).

### General prerequisites

To use the C# client libraries, you need to have at least version 2.2 of the [.NET Core](https://dotnet.microsoft.com/download/dotnet-core/2.2) install on your locale machine. To verify the version of your .NET Core Software Development Kit, type:

```bash
$ dotnet --version
```

The output needs to show a version number later than "2.2.100" for the basic examples.

You also need to have a valid tenant, user and password in order to access Cumulocity. Refer to [Tenant ID and tenant domain](https://cumulocity.com/guides/reference/tenants/#tenant-id-and-domain) in the Reference guide to learn the difference between tenant ID and tenant domain.
