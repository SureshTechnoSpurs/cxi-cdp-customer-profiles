FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["ClientWebAppService.UserProfile/ClientWebAppService.UserProfile.csproj", "ClientWebAppService.UserProfile/"]
COPY ["NuGet.Config","NuGet.Config"]

ARG PAT
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true

RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | bash
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS \
   "{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/Customerxi/7744d0ac-6bc1-4039-b0b8-2543d12c2f99/_packaging/cxi/nuget/v3/index.json\",\"username\": \"docker\", \"password\":\"${PAT}\"}]}"

RUN dotnet restore "ClientWebAppService.UserProfile/ClientWebAppService.UserProfile.csproj" --configfile NuGet.Config

WORKDIR "/src/ClientWebAppService.UserProfile"
RUN dotnet build "ClientWebAppService.UserProfile.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientWebAppService.UserProfile.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientWebAppService.UserProfile.dll"]