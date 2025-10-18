#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./scraper/ ./scraper/
COPY ./MapleTinder.Shared/ ./MapleTinder.Shared/

WORKDIR /src/scraper
RUN dotnet restore

# Publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the scraper
ENTRYPOINT ["dotnet", "scraper.dll"]
