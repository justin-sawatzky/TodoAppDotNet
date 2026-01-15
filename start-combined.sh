#!/bin/sh

# Start the .NET backend in the background
cd /app/backend
export ASPNETCORE_URLS=http://+:5247
export ASPNETCORE_ENVIRONMENT=Production
export UseInMemoryDatabase=true
export DebugSaveToDisk=false
dotnet TodoAppDotNet.dll &

# Wait a moment for backend to start
sleep 5

# Start nginx in the foreground
nginx -g "daemon off;"