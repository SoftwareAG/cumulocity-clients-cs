
FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY ./publish/Web ./

ENV SERVER_PORT 4700
ENV C8Y_BASEURL "http://management.staging7.c8y.io"
ENV C8Y_BOOTSTRAP_TENANT "management"
ENV C8Y_BOOTSTRAP_USERNAME "servicebootstrap_cs-combain"
ENV C8Y_BOOTSTRAP_PASSWORD "qoGzAMCoXF"
ENV C8Y_MICROSERIVCE_ISOLATION "PER_TENANT"

ENTRYPOINT ["dotnet", "DockerWebApp.Demo.dll"]
