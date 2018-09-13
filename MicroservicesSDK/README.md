
##How to release a new version of SDK

The tagging a new release should be preceded the bumping the version to required number.
After that the new tag should be created and next the pushing code changes could be done.

The  required format of tag is "prerelease{{tag}}" where the tag is a version number X.X.X e.g.:
```
 hg tag prerelease1.1.0
```

The next step is to start Jenkins' job - [Release](http://localhost:8081/view/C8Y-RELEASE/job/Docker-Cumulocity-Clients-CSharp-MicroserviceSdk-RELEASE)
To build a new release, use the develop parameter