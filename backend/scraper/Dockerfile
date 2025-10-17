FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ./scraper/ ./scraper/
COPY ./MapleTinder.Shared/ ./MapleTinder.Shared/

# Restore dependencies
WORKDIR /src/scraper
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the lightweight ASP.NET runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the scraper
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
