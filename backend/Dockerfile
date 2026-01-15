# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5247

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["TodoAppDotNet.csproj", "./"]
RUN dotnet restore "TodoAppDotNet.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "TodoAppDotNet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoAppDotNet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for container
ENV ASPNETCORE_URLS=http://+:5247
ENV ASPNETCORE_ENVIRONMENT=Production
ENV UseInMemoryDatabase=true
ENV DebugSaveToDisk=false

ENTRYPOINT ["dotnet", "TodoAppDotNet.dll"]