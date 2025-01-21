# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Yarp.Gateway/Yarp.Gateway.csproj", "Gateway/"]

# Restore NuGet packages
RUN dotnet restore "Gateway/Yarp.Gateway.csproj"

# Copy the rest of the source code
COPY ["Yarp.Gateway/", "Gateway/"]

# Build the application
WORKDIR "/src/Gateway"
RUN dotnet build "Yarp.Gateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Yarp.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15000
ENV ASPNETCORE_HTTPS_PORTS=17000

# Expose ports
EXPOSE 15000 17000

ENTRYPOINT ["dotnet", "Yarp.Gateway.dll"]