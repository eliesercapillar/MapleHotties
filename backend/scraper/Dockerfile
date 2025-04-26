FROM mcr.microsoft.com/playwright:v1.51.0-noble

# Install .NET SDK
RUN apt-get update && apt-get install -y wget apt-transport-https software-properties-common && \
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y dotnet-sdk-8.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# 👇 Copy both scraper and shared project folders into the build context
COPY ./scraper/ ./scraper/
COPY ./MapleTinder.Shared/ ./MapleTinder.Shared/

WORKDIR /app/scraper

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

WORKDIR /app/publish
ENTRYPOINT ["dotnet", "scraper.dll"]


#---------------------------------------------
#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app

# Build
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src

#COPY ./scraper/ ./scraper/
#COPY MapleTinder.Shared/ ./MapleTinder.Shared/

#WORKDIR /src/scraper
#RUN dotnet restore
#RUN dotnet build "./scraper.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src/scraper
#RUN dotnet publish "./scraper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .

# Set the entry point
#ENTRYPOINT ["dotnet", "scraper.dll"]
