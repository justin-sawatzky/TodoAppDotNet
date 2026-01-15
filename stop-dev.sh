#!/bin/bash

echo "🛑 Stopping Todo App servers..."

# Stop backend
BACKEND_PIDS=$(lsof -ti :5247)
if [ ! -z "$BACKEND_PIDS" ]; then
    echo "   Stopping backend (port 5247)..."
    kill $BACKEND_PIDS 2>/dev/null
    echo "   ✅ Backend stopped"
else
    echo "   ℹ️  Backend not running"
fi

# Stop frontend
FRONTEND_PIDS=$(lsof -ti :5173)
if [ ! -z "$FRONTEND_PIDS" ]; then
    echo "   Stopping frontend (port 5173)..."
    kill $FRONTEND_PIDS 2>/dev/null
    echo "   ✅ Frontend stopped"
else
    echo "   ℹ️  Frontend not running"
fi

echo ""
echo "✨ All servers stopped"
