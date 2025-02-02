# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["ClientManagement.API/Apis/ClientManagement.API.csproj", "ClientManagement/Apis/"]
COPY ["ClientManagement.API/Application/ClientManagement.Application.csproj", "ClientManagement/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "ClientManagement/Apis/ClientManagement.API.csproj"

# Copy the rest of the source code
COPY ["ClientManagement.API/Apis/", "ClientManagement/Apis/"]
COPY ["ClientManagement.API/Application/", "ClientManagement/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/ClientManagement/Apis"
RUN dotnet build "ClientManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ClientManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15305
ENV ASPNETCORE_HTTPS_PORTS=17305

# Expose ports
EXPOSE 15305 17305

ENTRYPOINT ["dotnet", "ClientManagement.API.dll"]