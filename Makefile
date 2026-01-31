# LeadFlowAI - Development Makefile
# Use with Git Bash, WSL, or compatible shell on Windows

.PHONY: help build test run dev up down clean docker-build docker-push deploy

# Default target
help:
	@echo "LeadFlowAI Development Commands"
	@echo ""
	@echo "Development:"
	@echo "  make build          - Build .NET projects"
	@echo "  make test           - Run all tests"
	@echo "  make run            - Run WebAPI locally"
	@echo "  make dev            - Start development environment"
	@echo ""
	@echo "Docker:"
	@echo "  make up             - Start all services with Docker Compose"
	@echo "  make down           - Stop all services"
	@echo "  make docker-build   - Build Docker images"
	@echo "  make docker-push    - Push images to registry"
	@echo ""
	@echo "Deployment:"
	@echo "  make deploy         - Deploy to production"
	@echo ""
	@echo "Maintenance:"
	@echo "  make clean          - Clean build artifacts"
	@echo "  make logs           - Show service logs"

# Build .NET projects
build:
	dotnet build LeadFlowAI.sln

# Run tests
test:
	dotnet test LeadFlowAI.sln

# Run WebAPI locally
run:
	cd src/LeadFlowAI.WebAPI && dotnet run

# Start development environment
dev:
	docker-compose -f docker-compose.dev.yml --env-file .env.dev up -d
	@echo "Development environment started!"
	@echo "WebAPI: http://localhost:8080"
	@echo "Swagger: http://localhost:8080/swagger"
	@echo "PgAdmin: http://localhost:5050"
	@echo "Redis Commander: http://localhost:8081"

# Docker Compose commands
up:
	docker-compose -f docker-compose.dev.yml --env-file .env.dev up -d

down:
	docker-compose -f docker-compose.dev.yml --env-file .env.dev down

# Docker image management
docker-build:
	docker build -f src/LeadFlowAI.WebAPI/Dockerfile -t leadflowai/webapi:latest .
	docker build -f src/LeadFlowAI.Worker/Dockerfile -t leadflowai/worker:latest .

docker-push:
	@echo "Pushing to registry (configure DOCKER_REGISTRY in .env)"
	docker tag leadflowai/webapi:latest $(DOCKER_REGISTRY)/webapi:latest
	docker tag leadflowai/worker:latest $(DOCKER_REGISTRY)/worker:latest
	docker push $(DOCKER_REGISTRY)/webapi:latest
	docker push $(DOCKER_REGISTRY)/worker:latest

# Production deployment
deploy:
	./deploy.sh deploy

# Clean build artifacts
clean:
	dotnet clean LeadFlowAI.sln
	docker system prune -f

# Show logs
logs:
	docker-compose -f docker-compose.dev.yml --env-file .env.dev logs -f