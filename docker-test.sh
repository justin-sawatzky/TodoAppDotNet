#!/bin/bash

echo "🐳 Testing Docker Setup..."

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

echo "✅ Docker is available"

# Test backend Dockerfile
echo "🔨 Testing backend Dockerfile..."
if docker build -t todoapp-backend-test ./backend; then
    echo "✅ Backend Docker build successful"
else
    echo "❌ Backend Docker build failed"
    exit 1
fi

# Test frontend Dockerfile
echo "🔨 Testing frontend Dockerfile..."
if docker build -t todoapp-frontend-test ./frontend/TodoAppFrontend; then
    echo "✅ Frontend Docker build successful"
else
    echo "❌ Frontend Docker build failed"
    exit 1
fi

# Test combined Dockerfile
echo "🔨 Testing combined Dockerfile..."
if docker build -t todoapp-combined-test .; then
    echo "✅ Combined Docker build successful"
else
    echo "❌ Combined Docker build failed"
    exit 1
fi

echo ""
echo "🎉 All Docker builds successful!"
echo ""
echo "To run the containers:"
echo "  Backend:  docker run -p 5247:5247 todoapp-backend-test"
echo "  Frontend: docker run -p 80:80 todoapp-frontend-test"
echo "  Combined: docker run -p 80:80 -p 5247:5247 todoapp-combined-test"
echo ""
echo "Or use docker-compose:"
echo "  docker-compose up --build"

# Clean up test images
echo "🧹 Cleaning up test images..."
docker rmi todoapp-backend-test todoapp-frontend-test todoapp-combined-test 2>/dev/null || true

echo "✅ Docker setup is ready!"