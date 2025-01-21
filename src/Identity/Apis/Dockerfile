# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Identity/Apis/Identity.API.csproj", "Identity/Apis/"]
COPY ["Identity/Application/Identity.Application.csproj", "Identity/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "Identity/Apis/Identity.API.csproj"

# Copy the rest of the source code
COPY ["Identity/Apis/", "Identity/Apis/"]
COPY ["Identity/Application/", "Identity/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/Identity/Apis"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15105
ENV ASPNETCORE_HTTPS_PORTS=17105

# Expose ports
EXPOSE 15105 17105

ENTRYPOINT ["dotnet", "Identity.API.dll"]