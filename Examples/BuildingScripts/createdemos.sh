#!/bin/sh
echo Hello World
whoami
pwd
cd microservicesdk-win-dev/
pwd
pwsh ./create.ps1 demo api
cd ../../..

chmod +x ./Examples/BuildingScripts/microservicesdk-lin-dev/create.sh
cd Examples/BuildingScripts/microservicesdk-lin-dev/
pwd
./create.sh demo api

cd ../../..

#rm -rf Examples/BuildingScripts/*.zip
#rm -rf Examples/BuildingScripts/*.zip

#zip -r Examples/BuildingScripts/microservicesdk-1.0.0.0-win.zip Examples/BuildingScripts/microservicesdk-1.0.0.0-win-dev/
#zip -r Examples/BuildingScripts/microservicesdk-1.0.0.0-lin.zip Examples/BuildingScripts/microservicesdk-1.0.0.0-lin-dev/

