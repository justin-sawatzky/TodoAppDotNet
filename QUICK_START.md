# Quick Start Guide

## Option 1: Using Separate Terminals (Recommended)

### Terminal 1 - Backend
```bash
cd backend
dotnet run --urls "http://localhost:5247"
```

### Terminal 2 - Frontend
```bash
cd frontend/TodoAppFrontend
npm run dev
```

## Option 2: Using Scripts

### Start Backend Only
```bash
./start-backend.sh
```

### Start Frontend Only (in another terminal)
```bash
./start-frontend.sh
```

### Stop All Servers
```bash
./stop-dev.sh
```

## Verify Servers Are Running

### Check Backend
```bash
curl http://localhost:5247/users
```

### Check Frontend
Open your browser to: http://localhost:5173

## Troubleshooting

### Port Already in Use

If you get "address already in use" errors:

**Backend (port 5247):**
```bash
lsof -ti :5247 | xargs kill
```

**Frontend (port 5173):**
```bash
lsof -ti :5173 | xargs kill
```

### Backend Won't Start
```bash
cd backend
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5247"
```

### Frontend Won't Start
```bash
cd frontend/TodoAppFrontend
npm install
npm run dev
```

### Database Issues
```bash
cd backend
# Delete database and recreate
rm todoapp.db todoapp.db-shm todoapp.db-wal
dotnet tool run dotnet-ef database update
```

## Current Status

Your servers are currently running via background processes:
- Backend: http://localhost:5247 ✅
- Frontend: http://localhost:5173 ✅

Just open http://localhost:5173 in your browser to use the app!
