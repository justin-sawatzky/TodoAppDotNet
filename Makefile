# TodoApp Docker Makefile

.PHONY: help build run stop clean test

help: ## Show this help message
	@echo "TodoApp Docker Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

build: ## Build all Docker images
	docker-compose build --no-cache

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

test: ## Test Docker builds
	./docker-test.sh

# Individual service commands
build-backend: ## Build only backend
	docker build -t todoapp-backend ./backend

build-frontend: ## Build only frontend
	docker build -t todoapp-frontend ./frontend/TodoAppFrontend

build-combined: ## Build combined container
	docker build -t todoapp-combined .

run-backend: ## Run only backend
	docker run -d -p 5247:5247 --name todoapp-backend todoapp-backend

run-frontend: ## Run only frontend
	docker run -d -p 80:80 --name todoapp-frontend todoapp-frontend

run-combined: ## Run combined container
	docker run -d -p 80:80 -p 5247:5247 --name todoapp-combined todoapp-combined

# Development commands
dev: ## Start development environment
	docker-compose up --build

dev-logs: ## Follow development logs
	docker-compose logs -f

dev-shell-backend: ## Access backend container shell
	docker exec -it todoapp-backend /bin/bash

dev-shell-frontend: ## Access frontend container shell
	docker exec -it todoapp-frontend /bin/sh