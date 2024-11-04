#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk update && apk --no-cache add icu-libs fontconfig ttf-dejavu wget
WORKDIR /app
EXPOSE 80
EXPOSE 443
HEALTHCHECK --start-period=30s  --interval=30s --timeout=5s CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DockerRegistryUI/TrivyAPIClient.csproj", "TrivyAPIClient/"]
COPY ["DockerRegistryUI/DockerRegistryUI.csproj", "DockerRegistryUI/"]
COPY ["DockerRegistry/DockerRegistry.csproj", "DockerRegistry/"]
RUN dotnet restore "DockerRegistryUI/DockerRegistryUI.csproj"
COPY . .
WORKDIR "/src/DockerRegistryUI"
RUN dotnet build "DockerRegistryUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DockerRegistryUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerRegistryUI.dll"]