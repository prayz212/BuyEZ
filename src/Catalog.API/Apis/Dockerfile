# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Catalog.API/Apis/Catalog.API.csproj", "Catalog/Apis/"]
COPY ["Catalog.API/Application/Catalog.Application.csproj", "Catalog/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "Catalog/Apis/Catalog.API.csproj"

# Copy the rest of the source code
COPY ["Catalog.API/Apis/", "Catalog/Apis/"]
COPY ["Catalog.API/Application/", "Catalog/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/Catalog/Apis"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/build /src/build

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15205
ENV ASPNETCORE_HTTPS_PORTS=17205

# Expose ports
EXPOSE 15205 17205

ENTRYPOINT ["dotnet", "Catalog.API.dll"]