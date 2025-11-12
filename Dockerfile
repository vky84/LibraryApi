# Root multi-stage Dockerfile for the whole solution
# Builds two publish outputs and exposes two final targets:
#  - libraryapi (dotnet LibraryApi.dll)
#  - notificationservice (dotnet NotificationService.dll)

# Build stage: restore & publish both projects
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything into the build context
COPY . .

# Restore (uses project-specific restore to keep it explicit)
RUN dotnet restore "LibraryApi/LibraryApi.csproj"
RUN dotnet restore "NotificationService/NotificationService.csproj"

# Publish LibraryApi
RUN dotnet publish "LibraryApi/LibraryApi.csproj" -c Release -o /app/libraryapi /p:UseAppHost=false

# Publish NotificationService
RUN dotnet publish "NotificationService/NotificationService.csproj" -c Release -o /app/notificationservice /p:UseAppHost=false

# Runtime image for LibraryApi (named target)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS libraryapi
WORKDIR /app
COPY --from=build /app/libraryapi .
ENTRYPOINT ["dotnet", "LibraryApi.dll"]

# Runtime image for NotificationService (named target)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS notificationservice
WORKDIR /app
COPY --from=build /app/notificationservice .
# NotificationService listens on 8080 in your Dockerfile; keep EXPOSE for documentation
EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationService.dll"]
