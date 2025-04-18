FROM mcr.microsoft.com/playwright:v1.52.0-noble

# Install .NET SDK
RUN apt-get update && apt-get install -y wget apt-transport-https software-properties-common && \
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && apt-get install -y dotnet-sdk-8.0 && \
    rm -rf /var/lib/apt/lists/*

# Set up app directory
WORKDIR /app

# Copy and build
COPY . . 
RUN dotnet publish "scraper.csproj" -c Release -o /app/publish

# Set working dir and entrypoint
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "scraper.dll"]
