#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official Playwright Docker image with .NET SDK
# Build
FROM mcr.microsoft.com/playwright:v1.52.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api.csproj", "."]
RUN dotnet restore "./api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN npx playwright install
RUN npm i
RUN pwsh bin/Debug/netX/playwright.ps1 install  

# Set the entry point
ENTRYPOINT ["dotnet", "api.dll"]