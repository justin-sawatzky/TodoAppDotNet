#!/bin/bash

echo "🚀 Starting backend server..."
cd backend
dotnet run --urls "http://localhost:5247"
