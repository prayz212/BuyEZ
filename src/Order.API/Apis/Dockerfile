# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files first
# This is done separately to take advantage of Docker's layer caching
COPY ["Order.API/Apis/Order.API.csproj", "Order/Apis/"]
COPY ["Order.API/Application/Order.Application.csproj", "Order/Application/"]
COPY ["Shared/BuyEZ.Shared.csproj", "Shared/"]

# Restore NuGet packages
RUN dotnet restore "Order/Apis/Order.API.csproj"

# Copy the rest of the source code
COPY ["Order.API/Apis/", "Order/Apis/"]
COPY ["Order.API/Application/", "Order/Application/"]
COPY ["Shared/", "Shared/"]

# Build the application
WORKDIR "/src/Order/Apis"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=15405
ENV ASPNETCORE_HTTPS_PORTS=17405

# Expose ports
EXPOSE 15405 17405

ENTRYPOINT ["dotnet", "Order.API.dll"]