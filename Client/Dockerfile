
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY Client.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0.0 AS runtime
WORKDIR /app
#COPY Client.runtimeconfig.json .
COPY --from=build /app/publish .
RUN apt-get update && apt-get install -y vim
#CMD ["/bin/bash"]
ENTRYPOINT [ "dotnet", "Client.dll" ]