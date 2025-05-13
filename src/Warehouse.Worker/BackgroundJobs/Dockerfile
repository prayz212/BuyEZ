# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Warehouse.Worker/BackgroundJobs/Warehouse.BackgroundJob.csproj", "Warehouse/BackgroundJobs/"]
COPY ["Warehouse.Worker/Application/Warehouse.Application.csproj", "Warehouse/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "Warehouse/BackgroundJobs/Warehouse.BackgroundJob.csproj"

# Copy the rest of the source code
COPY ["Warehouse.Worker/BackgroundJobs/", "Warehouse/BackgroundJobs/"]
COPY ["Warehouse.Worker/Application/", "Warehouse/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/Warehouse/BackgroundJobs"
RUN dotnet build "Warehouse.BackgroundJob.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Warehouse.BackgroundJob.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15605
ENV ASPNETCORE_HTTPS_PORTS=17605

# Expose ports
EXPOSE 15605 17605

ENTRYPOINT ["dotnet", "Warehouse.BackgroundJob.dll"]