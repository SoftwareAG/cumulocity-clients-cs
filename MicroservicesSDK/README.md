

##How to release a new version of SDK with scripts (Scripted)

The bumping the version number to required and next to tag it. Example:

```
 .\prerelease.ps1 -fixVersion "1.1.0"
```

The script adds the tag and changes the versions.

##How to release a new version of SDK (Manually)

The tagging a new release should be preceded the bumping the version to required number.

```
   <PropertyGroup>
     <TargetFramework>netcoreapp2.0</TargetFramework>
     <Version>1.1.0</Version>
   </PropertyGroup>
```

After that the new tag should be created and next the pushing code changes could be done.

The  required format of tag is "prerelease{{tag}}" where the tag is a version number X.X.X e.g.:
```
 hg tag prerelease1.1.0
```

The next step is to start Jenkins' job - [Release](http://localhost:8081/view/C8Y-RELEASE/job/Docker-Cumulocity-Clients-CSharp-MicroserviceSdk-RELEASE)
To build a new release, use the develop parameter