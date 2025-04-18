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
