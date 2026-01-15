#!/bin/bash

# Function to check if a port is in use
check_port() {
    lsof -i :$1 > /dev/null 2>&1
    return $?
}

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "Stopping servers..."
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null
    fi
    exit
}

trap cleanup INT TERM

# Check if backend is already running
if check_port 5247; then
    echo "⚠️  Backend already running on port 5247"
    echo "   Skipping backend startup..."
else
    echo "🚀 Starting backend server..."
    cd backend
    dotnet run --urls "http://localhost:5247" > /dev/null 2>&1 &
    BACKEND_PID=$!
    cd ..
    
    # Wait for backend to start
    echo "   Waiting for backend to start..."
    sleep 3
    
    if check_port 5247; then
        echo "✅ Backend started successfully"
    else
        echo "❌ Backend failed to start"
        exit 1
    fi
fi

# Check if frontend is already running
if check_port 5173; then
    echo "⚠️  Frontend already running on port 5173"
    echo "   Skipping frontend startup..."
else
    echo "🚀 Starting frontend server..."
    cd frontend/TodoAppFrontend
    npm run dev > /dev/null 2>&1 &
    FRONTEND_PID=$!
    cd ../..
    
    # Wait for frontend to start
    echo "   Waiting for frontend to start..."
    sleep 3
    
    if check_port 5173; then
        echo "✅ Frontend started successfully"
    else
        echo "❌ Frontend failed to start"
        cleanup
        exit 1
    fi
fi

echo ""
echo "================================"
echo "✨ Todo App is running!"
echo "================================"
echo "Backend:  http://localhost:5247"
echo "Frontend: http://localhost:5173"
echo ""
echo "Press Ctrl+C to stop servers"
echo ""

# Keep script running
if [ ! -z "$BACKEND_PID" ] || [ ! -z "$FRONTEND_PID" ]; then
    wait
else
    echo "Both servers were already running."
    echo "To stop them, use:"
    echo "  pkill -f 'dotnet run'"
    echo "  pkill -f 'vite'"
fi
