# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Shipping.Worker/BackgroundJobs/Shipping.BackgroundJob.csproj", "Shipping/BackgroundJobs/"]
COPY ["Shipping.Worker/Application/Shipping.Application.csproj", "Shipping/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "Shipping/BackgroundJobs/Shipping.BackgroundJob.csproj"

# Copy the rest of the source code
COPY ["Shipping.Worker/BackgroundJobs/", "Shipping/BackgroundJobs/"]
COPY ["Shipping.Worker/Application/", "Shipping/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/Shipping/BackgroundJobs"
RUN dotnet build "Shipping.BackgroundJob.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Shipping.BackgroundJob.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15505
ENV ASPNETCORE_HTTPS_PORTS=17505

# Expose ports
EXPOSE 15505 17505

ENTRYPOINT ["dotnet", "Shipping.BackgroundJob.dll"]