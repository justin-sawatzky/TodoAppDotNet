# TodoApp Docker Makefile

.PHONY: help build run stop clean test smithy-build smithy-validate smithy-clean generate-openapi generate-frontend-types generate-backend-types generate-all format format-frontend format-backend lint lint-frontend lint-backend test-api test-api-ts test-api-bash test-api-powershell test-api-install

help: ## Show this help message
	@echo "TodoApp Docker Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

build: ## Build all Docker images
	docker-compose build --no-cache

# Smithy and Code Generation Commands
smithy-build: ## Build Smithy model and generate OpenAPI spec
	@echo "Building Smithy model..."
	smithy build
	@echo "✅ Smithy model built successfully"

smithy-validate: ## Validate Smithy model without building
	@echo "Validating Smithy model..."
	smithy validate model
	@echo "✅ Smithy model validation complete"

smithy-clean: ## Clean Smithy build artifacts
	@echo "Cleaning Smithy build artifacts..."
	rm -rf build/smithy
	@echo "✅ Smithy build artifacts cleaned"

generate-openapi: smithy-build ## Generate OpenAPI specification from Smithy model
	@echo "✅ OpenAPI specification generated at build/smithy/source/openapi/TodoAppDotNet.openapi.json"

generate-frontend-types: generate-openapi ## Generate TypeScript types for frontend from OpenAPI spec
	@echo "Generating TypeScript types for frontend..."
	cd frontend/TodoAppFrontend && npm run generate-api
	@echo "✅ Frontend TypeScript types generated"

generate-backend-types: generate-openapi ## Generate C# DTOs for backend from OpenAPI spec
	@echo "Generating C# DTOs for backend..."
	@echo "Installing/updating NSwag.ConsoleCore tool locally..."
	dotnet tool install NSwag.ConsoleCore --tool-path ./tools || dotnet tool update NSwag.ConsoleCore --tool-path ./tools
	@echo "Running NSwag code generation..."
	@mkdir -p backend/Generated
	./tools/nswag openapi2csclient \
		/input:build/smithy/source/openapi/TodoAppDotNet.openapi.json \
		/output:backend/Generated/ApiModels.cs \
		/namespace:TodoApp.Generated \
		/generateClientClasses:false \
		/generateClientInterfaces:false \
		/generateDtoTypes:true \
		/generateOptionalPropertiesAsNullable:true \
		/generateNullableReferenceTypes:true \
		/jsonLibrary:NewtonsoftJson \
		/dateTimeType:DateTimeOffset
	@echo "✅ Backend C# DTOs generated"

generate-all: generate-frontend-types generate-backend-types ## Generate all code from Smithy model (OpenAPI + Frontend types + Backend DTOs)
	@echo "✅ All code generation complete"

# Formatting and Linting Commands
format: format-backend format-frontend ## Format all code (backend + frontend)

format-backend: ## Format backend C# code
	@echo "Formatting backend C# code..."
	cd backend && dotnet format
	@echo "✅ Backend code formatted"

format-frontend: ## Format frontend TypeScript/React code
	@echo "Formatting frontend code..."
	cd frontend/TodoAppFrontend && npm run format
	@echo "✅ Frontend code formatted"

lint: lint-backend lint-frontend ## Lint all code (backend + frontend)

lint-backend: ## Lint backend C# code
	@echo "Linting backend C# code..."
	cd backend && dotnet format --verify-no-changes
	@echo "✅ Backend code linting passed"

lint-frontend: ## Lint frontend TypeScript/React code
	@echo "Linting frontend code..."
	cd frontend/TodoAppFrontend && npm run lint
	@echo "Checking frontend code formatting..."
	cd frontend/TodoAppFrontend && npm run format:check
	@echo "✅ Frontend code linting passed"

run: ## Run the application with docker-compose
	docker-compose up -d --build

stop: ## Stop all containers
	docker-compose down

logs: ## View logs from all services
	docker-compose logs -f

restart: ## Restart all services
	docker-compose restart

clean: ## Remove all containers and images
	docker-compose down --volumes --remove-orphans
	docker system prune -f

# Individual service commands
build-backend: ## Build only backend
	docker build -t todoapp-backend ./backend

build-frontend: ## Build only frontend
	docker build -t todoapp-frontend ./frontend/TodoAppFrontend

run-backend: ## Run only backend
	docker run -d -p 5247:5247 --name todoapp-backend todoapp-backend

run-frontend: ## Run only frontend
	docker run -d -p 80:80 --name todoapp-frontend todoapp-frontend

# Development commands
dev: ## Start development environment
	docker-compose up --build

dev-logs: ## Follow development logs
	docker-compose logs -f

dev-shell-backend: ## Access backend container shell
	docker exec -it todoapp-backend /bin/bash

dev-shell-frontend: ## Access frontend container shell
	docker exec -it todoapp-frontend /bin/sh

# API Testing Commands
API_BASE_URL ?= http://localhost:5247

test-api: test-api-ts ## Run API tests (uses TypeScript by default)

test-api-install: ## Install API test dependencies
	@echo "Installing API test dependencies..."
	cd tests && npm install
	@echo "✅ API test dependencies installed"

test-api-ts: ## Run API tests using TypeScript (cross-platform, requires Node.js 18+)
	@echo "Running API tests with TypeScript..."
	@cd tests && npx tsx api_test.ts --base-url $(API_BASE_URL)

test-api-bash: ## Run API tests using Bash/curl (macOS/Linux/Git Bash)
	@echo "Running API tests with Bash..."
	@bash tests/api_test.sh $(API_BASE_URL)

test-api-powershell: ## Run API tests using PowerShell (Windows/PowerShell Core)
	@echo "Running API tests with PowerShell..."
	@pwsh -ExecutionPolicy Bypass -File tests/api_test.ps1 -BaseUrl $(API_BASE_URL) 2>/dev/null || powershell -ExecutionPolicy Bypass -File tests/api_test.ps1 -BaseUrl $(API_BASE_URL)