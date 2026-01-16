# TodoAppDotNet

A modern full-stack Todo application demonstrating API-first development with Smithy modeling, .NET backend, and React TypeScript frontend. The project showcases clean architecture principles, type-safe API contracts, and automated code generation from a single source of truth.

## What is This Project?

TodoAppDotNet is a production-ready task management application that allows users to create accounts, manage multiple todo lists, and organize tasks with full CRUD operations. The application uses Smithy for API modeling, which generates OpenAPI specifications that drive both backend DTOs and frontend TypeScript types, ensuring end-to-end type safety and contract consistency.

Key architectural highlights:
- **API-First Design**: Smithy models define the API contract, generating OpenAPI specs
- **Type Safety**: Fully typed from database to UI with generated code
- **Clean Architecture**: Separation of concerns with Controllers, Services, and Repositories
- **Modern Stack**: .NET 9, React 19, TypeScript, and SQLite

## Design Philosophy & Trade-offs

This project was built as a technical assessment, so the technology choices reflect a balance between demonstrating competence with familiar tools and keeping the scope manageable for reviewers.

### Why These Technologies?

**Smithy for API Modeling**: I chose Smithy because I have hands-on experience with it and knew I could leverage its code generation capabilities effectively. The upfront investment in modeling pays off through generated OpenAPI specs, TypeScript types, and C# DTOs — reducing boilerplate and eliminating contract drift between frontend and backend. For a greenfield project, alternatives like OpenAPI-first or code-first approaches are valid, but Smithy's strong typing and extensibility made it a natural fit.

**Docker Compose over Kubernetes**: This application is designed to run locally for assessment purposes, not deployed to cloud infrastructure. Docker Compose provides sufficient orchestration for a multi-container setup without the operational complexity of Kubernetes. For a production deployment, Kubernetes would be worth considering if we needed auto-scaling, rolling deployments, or multi-region distribution.

**Mono-repo Structure**: For a personal or team project, I'd likely split this into separate packages (model, backend, frontend, tests) with independent versioning and CI pipelines. However, a mono-repo makes it easier for assessors to clone once and see the full picture. The clean separation of concerns within the repo means extraction would be straightforward if needed.

### What's Intentionally Out of Scope

**CI/CD Pipelines**: Not implemented, but the project structure supports easy integration with GitHub Actions, GitLab CI, or similar. The Makefile commands (`make lint`, `make test-api`, `make generate-all`) map directly to pipeline stages.

**Authentication & Authorization**: The API currently has no auth layer. In a production environment, I'd integrate with whatever identity provider the organization uses (OAuth2/OIDC, JWT-based auth, etc.) rather than being prescriptive about a specific solution. The clean controller/service separation makes adding an auth middleware straightforward.

**Production Database**: SQLite is embedded for zero-config local development. See the Scalability section for production database considerations.

## Potential Enhancements

If this were a production application, here are features I'd consider adding:

### User-Facing Features
- **Due dates and reminders**: Add optional due dates to tasks with email/push notification reminders
- **Recurring tasks**: Support for daily, weekly, or custom recurrence patterns
- **List sharing**: Allow users to share todo lists with collaborators
- **Export functionality**: Export lists as CSV, JSON, or printable formats

### Backend Improvements
- **Authorization layer**: Integrate with an identity provider to secure endpoints and support multi-tenancy
- **Database migration**: Move from SQLite to a production-grade database (see Scalability section)
- **Audit logging**: Track changes for compliance and debugging
- **Rate limiting**: Protect against abuse with request throttling

## Scalability Considerations

This application is architected for local development and assessment, but here's how it could scale for production use:

### Current Limitations
- **SQLite**: Single-file database, no concurrent write scaling, not suitable for distributed deployments
- **Single instance**: The Docker Compose setup runs one backend container with no load balancing
- **In-process state**: No distributed caching or session management

### Horizontal Scaling Path

**Database Layer**: Replace SQLite with a dedicated database server. Two approaches depending on access patterns:

1. **Relational (PostgreSQL/MySQL)**: Good for complex queries, reporting, and if the data model grows more relational. Supports read replicas for scaling reads.

2. **Key-Value/Document Store (DynamoDB, MongoDB)**: Better for high-volume, simple access patterns like "get all tasks for user X." Todo data is naturally document-shaped (user → lists → tasks), and sharding by user ID provides straightforward horizontal scaling. This trades query flexibility for predictable performance at scale.

**Application Layer**: 
- Run multiple backend instances behind a load balancer
- Ensure stateless request handling (already the case — no server-side sessions)
- Add distributed caching (Redis) for frequently accessed data

## Technical Requirements

### macOS / Linux

**Required (for Docker-based setup - recommended):**
- **Docker Desktop** 20.10+ or **Docker Engine** 20.10+ - [Install Docker](https://docs.docker.com/get-docker/)
- **Docker Compose** 2.0+ - [Install Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)
- **Git** - [Install Git](https://git-scm.com/downloads)

**Required (for local development without Docker):**
- **.NET SDK** 9.0 or later - [Install .NET](https://dotnet.microsoft.com/download)
- **Node.js** 22.x (LTS) or later - [Install Node.js](https://nodejs.org/)
- **npm** 10.x or later (comes with Node.js)
- **Smithy CLI** - [Install Smithy](https://smithy.io/2.0/guides/smithy-cli/cli_installation.html) (required for API code generation)
- **Git** - [Install Git](https://git-scm.com/downloads)

**Optional tools:**
- **Make**: GNU Make (pre-installed on most systems) - for convenient Makefile commands
- **mise**: For managing Node.js versions - [Install mise](https://mise.jdx.dev/getting-started.html) (configured in `.mise.toml`)

### Windows

**Required (for Docker-based setup - recommended):**
- **Docker Desktop** 20.10+ with WSL2 backend - [Install Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/)
- **Docker Compose** 2.0+ - [Install Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)
- **WSL2** (Windows Subsystem for Linux) - [Install WSL2](https://docs.microsoft.com/en-us/windows/wsl/install) (required for Docker Desktop)
- **Git** for Windows 2.30+ - [Install Git](https://git-scm.com/download/win)

**Required (for local development without Docker):**
- **.NET SDK** 9.0 or later - [Install .NET](https://dotnet.microsoft.com/download)
- **Node.js** 22.x (LTS) or later - [Install Node.js](https://nodejs.org/)
- **npm** 10.x or later (comes with Node.js)
- **Smithy CLI** - [Install Smithy](https://smithy.io/2.0/guides/smithy-cli/cli_installation.html) (required for API code generation)
- **Git** for Windows 2.30+ - [Install Git](https://git-scm.com/download/win)

**Optional tools:**
- **PowerShell** 7+ or Windows Terminal - [Install PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)
- **Make for Windows**: GNU Make via [Chocolatey](https://chocolatey.org/), [Scoop](https://scoop.sh/), or WSL2

**Note for Windows users:** You can run all commands using PowerShell or Command Prompt. For Makefile commands, either install GNU Make for Windows or use the underlying commands directly (shown in each section).

## Building and Running from Scratch

### Option 1: Docker (Recommended - Works on All Platforms)

This is the fastest way to get started and requires minimal setup.

1. **Clone the repository**
```bash
git clone <repository-url>
cd TodoAppDotNet
```

2. **Start the application**
```bash
# Using Docker Compose (recommended)
docker-compose up --build

# Or using Make (macOS/Linux/WSL2)
make run
```

3. **Access the application**
- Frontend: http://localhost:80
- Backend API: http://localhost:5247

4. **Stop the application**
```bash
# Stop containers
docker-compose down

# Or using Make
make stop
```

### Option 2: Local Development (Without Docker)

This approach gives you more control and faster iteration during development.

#### Step 1: Install Prerequisites

**macOS/Linux:**
```bash
# Install .NET SDK (if not already installed)
# Visit: https://dotnet.microsoft.com/download

# Install Node.js (if not already installed)
# Visit: https://nodejs.org/ or use mise:
mise install

# Verify installations
dotnet --version  # Should show 9.0.x or later
node --version    # Should show v22.x or later
npm --version     # Should show 10.x or later
```

**Windows:**
```powershell
# Install .NET SDK
# Download from: https://dotnet.microsoft.com/download

# Install Node.js
# Download from: https://nodejs.org/

# Verify installations
dotnet --version  # Should show 9.0.x or later
node --version    # Should show v22.x or later
npm --version     # Should show 10.x or later
```

#### Step 2: Build the Backend

```bash
# Navigate to backend directory
cd backend

# Restore .NET dependencies
dotnet restore

# Build the project
dotnet build

# Initialize the database (creates SQLite database)
dotnet ef database update
# Or simply run the app - it will create the database automatically
```

#### Step 3: Build the Frontend

```bash
# Navigate to frontend directory
cd frontend/TodoAppFrontend

# Install npm dependencies
npm install

# Build the frontend (optional - for production)
npm run build
```

#### Step 4: Run the Application

You'll need two terminal windows/tabs:

**Terminal 1 - Backend:**
```bash
cd backend
dotnet run --urls "http://localhost:5247"
```

**Terminal 2 - Frontend:**
```bash
cd frontend/TodoAppFrontend
npm run dev
```

**Access the application:**
- Frontend: http://localhost:5173 (Vite dev server)
- Backend API: http://localhost:5247

### Option 3: Using Makefile Commands (macOS/Linux/WSL2)

The Makefile provides convenient shortcuts for common tasks:

```bash
# View all available commands
make help

# Start with Docker Compose
make run

# View logs
make logs

# Stop containers
make stop

# Build Smithy models and generate code
make generate-all

# Format code
make format

# Lint code
make lint

# Clean up Docker resources
make clean
```

## API Testing

The project includes a comprehensive API test suite that validates all backend endpoints. Tests are available in multiple formats for cross-platform compatibility.

### Running API Tests

**Prerequisites**: The backend must be running before executing tests.

```bash
# Start the backend (using Docker)
docker-compose up -d

# Or start locally
cd backend && dotnet run
```

**Using TypeScript (recommended - cross-platform):**
```bash
# First time only: install test dependencies
make test-api-install

# Run tests
make test-api
```

**Using Bash/curl (macOS/Linux/Git Bash):**
```bash
make test-api-bash
```

**Using PowerShell (Windows):**
```bash
make test-api-powershell
```

**Custom API URL:**
```bash
make test-api API_BASE_URL=http://localhost:8080
```

### Test Coverage

The test suite covers:
- **User Operations**: Create, Read, Update, Delete, List, Lookup by email
- **Todo List Operations**: Create, Read, Update, Delete, List
- **Todo Task Operations**: Create, Read, Update, Delete, List, Reorder
- **Error Handling**: Validation errors (400), Not found errors (404)

### Running Tests Directly

Without Make, you can run tests directly:

```bash
# TypeScript (requires Node.js 18+)
cd tests && npx tsx api_test.ts --base-url http://localhost:5247

# Bash
bash tests/api_test.sh http://localhost:5247

# PowerShell
pwsh -File tests/api_test.ps1 -BaseUrl http://localhost:5247
```

## API Development Workflow

When you need to modify the API contract:

1. **Edit Smithy models** in `model/` directory
2. **Generate OpenAPI spec**:
```bash
make generate-openapi
# Or manually: smithy build
```

3. **Generate TypeScript types** for frontend:
```bash
make generate-frontend-types
# Or manually: cd frontend/TodoAppFrontend && npm run generate-api
```

4. **Generate C# DTOs** for backend:
```bash
make generate-backend-types
```

5. **Implement changes** in backend controllers and frontend components

## Debugging and Troubleshooting

### Common Issues and Solutions

#### Port Already in Use

**Problem**: Error message like "Address already in use" or "Port 5247/80 is already allocated"

**Solution**:
```bash
# Find and kill process using the port (macOS/Linux)
lsof -ti:5247 | xargs kill -9
lsof -ti:80 | xargs kill -9

# Windows (PowerShell)
Get-Process -Id (Get-NetTCPConnection -LocalPort 5247).OwningProcess | Stop-Process
Get-Process -Id (Get-NetTCPConnection -LocalPort 80).OwningProcess | Stop-Process

# Or change ports in docker-compose.yml or when running locally
```

#### Docker Build Fails

**Problem**: Docker build fails with network or dependency errors

**Solution**:
```bash
# Clear Docker cache and rebuild
docker-compose down --volumes
docker system prune -f
docker-compose up --build --force-recreate

# Or using Make
make clean
make run
```

#### Frontend Can't Connect to Backend

**Problem**: API calls fail with network errors

**Solution**:
- Verify backend is running: `curl http://localhost:5247/users`
- Check backend logs for errors
- In development, the Vite dev server proxies `/api` requests to the backend (configured in `vite.config.ts`)
- If using Docker, both services run on the same network
- If running the backend standalone without the Vite proxy, you may need to add CORS configuration to `Program.cs`

#### Database Migration Issues

**Problem**: Entity Framework migrations fail or database is out of sync

**Solution**:
```bash
cd backend

# Delete existing database
rm todoapp.db

# Recreate database
dotnet ef database update

# Or let the app create it automatically on startup
dotnet run
```

#### npm Install Fails

**Problem**: Frontend dependencies fail to install

**Solution**:
```bash
cd frontend/TodoAppFrontend

# Clear npm cache
npm cache clean --force

# Delete node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Reinstall
npm install
```

#### Smithy Build Fails

**Problem**: Smithy model validation or build errors

**Solution**:
```bash
# Validate Smithy model
make smithy-validate

# Check for syntax errors in model/*.smithy files
# Ensure all required dependencies are installed
npm install -g @smithy/cli

# Clean and rebuild
make smithy-clean
make smithy-build
```

#### Code Generation Issues

**Problem**: Generated TypeScript types or C# DTOs are outdated or missing

**Solution**:
```bash
# Regenerate all code from Smithy models
make generate-all

# Or step by step:
make generate-openapi
make generate-frontend-types
make generate-backend-types
```

### Viewing Logs

**Docker logs:**
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend

# Or using Make
make logs
```

**Local development:**
- Backend logs appear in the terminal where you ran `dotnet run`
- Frontend logs appear in the terminal where you ran `npm run dev`
- Browser console (F12) shows frontend runtime errors

### Debugging in IDE

**Backend (Visual Studio / VS Code / Rider):**
- Open `backend/TodoAppDotNet.csproj`
- Set breakpoints in Controllers, Services, or Repositories
- Press F5 to start debugging
- Backend will run on http://localhost:5247

**Frontend (VS Code / WebStorm):**
- Open `frontend/TodoAppFrontend` folder
- Use browser DevTools (F12) for debugging
- React DevTools extension recommended
- Set breakpoints in browser Sources tab

### Health Checks

Verify services are running correctly:

```bash
# Backend health check
curl http://localhost:5247/users

# Frontend (should return HTML)
curl http://localhost:80

# Docker container status
docker-compose ps
```

## Project Structure

```
.
├── backend/                            # .NET backend application
│   ├── Controllers/                    # API controllers
│   ├── Services/                       # Business logic layer
│   ├── Repositories/                   # Data access layer
│   ├── Models/                         # Entity models
│   ├── Data/                           # EF Core DbContext
│   ├── Generated/                      # Auto-generated C# DTOs from OpenAPI
│   ├── Program.cs                      # Application entry point
│   └── TodoAppDotNet.csproj            # Project file
├── frontend/
│   └── TodoAppFrontend/                # React TypeScript frontend
│       ├── src/
│       │   ├── api/                    # Generated TypeScript API client
│       │   ├── components/             # React components
│       │   └── types.ts                # TypeScript types
│       └── package.json
├── model/                              # Smithy API models
│   ├── main.smithy                     # Main service definition
│   └── resources/                      # Resource definitions
│   └── TodoAppDotNet.openapi.json      # Pre generated OpenAPI model file
├── tests/                              # API test suite
│   ├── api_test.ts                     # TypeScript tests (cross-platform)
│   ├── api_test.sh                     # Bash/curl tests (macOS/Linux)
│   ├── api_test.ps1                    # PowerShell tests (Windows)
│   └── package.json                    # Test dependencies
├── build/                              # Generated OpenAPI specs
├── docker-compose.yml                  # Multi-container orchestration
├── Makefile                            # Build and development commands
└── README.md                           # This file
```

## Features

- User account creation and authentication
- Multiple todo lists per user
- Task management with CRUD operations
- Mark tasks as complete/incomplete
- Drag-and-drop task reordering
- Filter tasks by completion status
- RESTful API with OpenAPI documentation

## Technologies

**Backend**: .NET 9, Entity Framework Core, SQLite, Smithy, OpenAPI 3.1  
**Frontend**: React 19, TypeScript, Vite, openapi-fetch, openapi-typescript  
**DevOps**: Docker, Docker Compose, Make

## License

MIT
