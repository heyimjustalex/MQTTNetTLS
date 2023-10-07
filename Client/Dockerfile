
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Client.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:6.0.0 AS runtime
RUN apt-get update && apt-get install -y vim
RUN apt-get install iputils-ping -y
RUN apt-get install -y tcpdump
WORKDIR /app
#COPY Client.runtimeconfig.json .
COPY --from=build /app/publish .
WORKDIR /app
COPY ./PKI/CA/rootCA.cer .
RUN chmod 777 /app/rootCA.cer
#CMD ["/bin/bash"]
ENTRYPOINT [ "dotnet", "Client.dll" ]