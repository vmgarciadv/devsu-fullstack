# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["devsu.csproj", "./"]
RUN dotnet restore "devsu.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "devsu.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "devsu.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Copy appsettings.Docker.json if exists
COPY appsettings.Docker.json* ./

# Set environment variable
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Run the application
ENTRYPOINT ["dotnet", "devsu.dll"]