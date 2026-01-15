# TodoAppDotNet

A full-stack Todo application with a .NET backend and React TypeScript frontend, using Smithy for API modeling and OpenAPI for code generation.

## Architecture

### Backend (.NET 10 + SQLite)
- **API Modeling**: Smithy → OpenAPI specification
- **Database**: SQLite with Entity Framework Core
- **Architecture**: Clean architecture with Controllers, Services, Repositories
- **API**: RESTful JSON API with full CRUD operations

### Frontend (React + TypeScript)
- **Client Generation**: TypeScript types generated from OpenAPI spec
- **Type Safety**: Fully typed API client using openapi-fetch
- **Features**: Login, list management, task management with drag-and-drop

## Project Structure

```
.
├── backend/
│   ├── model/              # Smithy API models
│   ├── Controllers/        # API controllers
│   ├── Services/           # Business logic
│   ├── Repositories/       # Data access layer
│   ├── Models/             # Entity models
│   ├── DTOs/               # Data transfer objects
│   ├── Data/               # EF Core DbContext
│   └── build/              # Generated OpenAPI spec
├── frontend/
│   └── TodoAppFrontend/
│       ├── src/
│       │   ├── api/        # Generated API client
│       │   ├── components/ # React components
│       │   └── types.ts    # TypeScript types
│       └── package.json
└── dotnet-tools.json       # Local .NET tools

```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 18+
- SQLite (included with .NET)

### Quick Start

**Option 1: Two Terminals (Recommended)**

Terminal 1 - Start Backend:
```bash
cd backend
dotnet run --urls "http://localhost:5247"
```

Terminal 2 - Start Frontend:
```bash
cd frontend/TodoAppFrontend
npm run dev
```

**Option 2: Using Scripts**
```bash
./start-backend.sh    # Terminal 1
./start-frontend.sh   # Terminal 2
./stop-dev.sh         # Stop all servers
```

Then open http://localhost:5173 in your browser!

### First Time Setup

1. **Backend Setup:**
```bash
cd backend
dotnet restore
dotnet tool run dotnet-ef database update
dotnet run --urls "http://localhost:5247"
```

2. **Frontend Setup:**
```bash
cd frontend/TodoAppFrontend
npm install
npm run dev
```

## API Development Workflow

### Modifying the API

1. **Update Smithy Models** in `backend/model/`
2. **Build Smithy** to generate OpenAPI:
```bash
cd backend
dotnet tool run smithy-cli build
```

3. **Regenerate Frontend Client**:
```bash
cd frontend/TodoAppFrontend
npm run generate-api
```

4. **Implement Backend Controllers** in `backend/Controllers/`
5. **Update Frontend Components** to use new API endpoints

## Features

### User Management
- Create user accounts with email and username
- Login with email
- User data persistence

### Todo Lists
- Create multiple todo lists per user
- View all lists in sidebar
- Delete lists (cascades to tasks)

### Tasks
- Add tasks to lists
- Mark tasks as complete/incomplete
- Edit task descriptions inline
- Delete individual tasks
- Drag and drop to reorder tasks
- Filter by completion status

## API Endpoints

### Users
- `GET /users` - List all users
- `POST /users` - Create a new user
- `GET /users/{userId}` - Get user by ID
- `PUT /users/{userId}` - Update user
- `DELETE /users/{userId}` - Delete user

### Todo Lists
- `GET /users/{userId}/lists` - List user's todo lists
- `POST /users/{userId}/lists` - Create a new list
- `GET /users/{userId}/lists/{listId}` - Get list by ID
- `PUT /users/{userId}/lists/{listId}` - Update list
- `DELETE /users/{userId}/lists/{listId}` - Delete list

### Tasks
- `GET /users/{userId}/lists/{listId}/tasks` - List tasks in a list
- `POST /users/{userId}/lists/{listId}/tasks` - Create a new task
- `GET /users/{userId}/lists/{listId}/tasks/{taskId}` - Get task by ID
- `PUT /users/{userId}/lists/{listId}/tasks/{taskId}` - Update task
- `DELETE /users/{userId}/lists/{listId}/tasks/{taskId}` - Delete task

## Technologies

### Backend
- .NET 10
- Entity Framework Core
- SQLite
- Smithy (API modeling)
- OpenAPI 3.1

### Frontend
- React 19
- TypeScript
- Vite
- openapi-fetch (type-safe API client)
- openapi-typescript (code generation)

## Database Schema

### Users
- `UserId` (PK)
- `Username`
- `Email` (unique)
- `CreatedAt`

### TodoLists
- `UserId` (PK, FK)
- `ListId` (PK)
- `Name`
- `Description`
- `CreatedAt`
- `UpdatedAt`

### TodoTasks
- `UserId` (PK, FK)
- `ListId` (PK, FK)
- `TaskId` (PK)
- `Description`
- `Completed`
- `CreatedAt`
- `UpdatedAt`

## License

MIT
A full stack TODO application written in .NET Core and React
