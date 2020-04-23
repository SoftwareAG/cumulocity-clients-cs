FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY ./publish/Web ./

#ENV SERVER_PORT 4700
#ENV C8Y_BASEURL "http://baseurl"
#ENV C8Y_BOOTSTRAP_TENANT "tenant"
#ENV C8Y_BOOTSTRAP_USERNAME "username"
#ENV C8Y_BOOTSTRAP_PASSWORD "password"
#ENV C8Y_MICROSERIVCE_ISOLATION "PER_TENANT"

ENTRYPOINT ["dotnet", "DockerApp.dll"]

