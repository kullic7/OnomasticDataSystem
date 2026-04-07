# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OnomasticsDataSystem.Web/OnomasticsDataSystem.Web.csproj", "OnomasticsDataSystem.Web/"]
COPY ["OnomasticsDataSystem.Core/OnomasticsDataSystem.Core.csproj", "OnomasticsDataSystem.Core/"]
COPY ["OnomasticsDataSystem.Infrastructure/OnomasticsDataSystem.Infrastructure.csproj", "OnomasticsDataSystem.Infrastructure/"]
COPY ["OnomasticsDataSystem.Common/OnomasticsDataSystem.Common.csproj", "OnomasticsDataSystem.Common/"]
RUN dotnet restore "./OnomasticsDataSystem.Web/OnomasticsDataSystem.Web.csproj"
COPY . .
WORKDIR "/src/OnomasticsDataSystem.Web"
RUN dotnet build "./OnomasticsDataSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OnomasticsDataSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnomasticsDataSystem.Web.dll"]