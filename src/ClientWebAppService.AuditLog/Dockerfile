FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["ClientWebAppService.AuditLog/ClientWebAppService.AuditLog.csproj", "ClientWebAppService.AuditLog/"]
COPY ["NuGet.Config","NuGet.Config"]

ARG PAT
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true

RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | bash
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS \
   "{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/Customerxi/7744d0ac-6bc1-4039-b0b8-2543d12c2f99/_packaging/cxi/nuget/v3/index.json\",\"username\": \"docker\", \"password\":\"${PAT}\"}]}"

RUN dotnet restore "ClientWebAppService.AuditLog/ClientWebAppService.AuditLog.csproj" --configfile NuGet.Config
COPY . .
WORKDIR "/src/ClientWebAppService.AuditLog"
RUN dotnet build "ClientWebAppService.AuditLog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientWebAppService.AuditLog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientWebAppService.AuditLog.dll"]