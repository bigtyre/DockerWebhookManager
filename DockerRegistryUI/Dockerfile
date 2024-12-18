FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk update && apk --no-cache add icu-libs fontconfig ttf-dejavu wget
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443
HEALTHCHECK --start-period=30s  --interval=30s --timeout=5s CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DockerRegistryUI/DockerRegistryUI.csproj", "DockerRegistryUI/"]
COPY ["DockerRegistry/DockerRegistry.csproj", "DockerRegistry/"]
COPY ["TrivyAPIClient/TrivyAPIClient.csproj", "TrivyAPIClient/"]
RUN dotnet restore "./DockerRegistryUI/DockerRegistryUI.csproj"
COPY . .
WORKDIR "/src/DockerRegistryUI"
RUN dotnet build "./DockerRegistryUI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DockerRegistryUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerRegistryUI.dll"]