#!/bin/bash

# LeadFlowAI - Production Deployment Script
# This script helps deploy LeadFlowAI to production environments

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
DOCKER_REGISTRY=${DOCKER_REGISTRY:-"ghcr.io/avila-inc/leadflowai"}
ENV_FILE=".env"

# Functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_dependencies() {
    log_info "Checking dependencies..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed. Please install Docker first."
        exit 1
    fi

    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose is not installed. Please install Docker Compose first."
        exit 1
    fi

    log_success "Dependencies check passed"
}

check_env_file() {
    if [ ! -f "$ENV_FILE" ]; then
        log_error "Environment file '$ENV_FILE' not found!"
        log_info "Please copy .env.example to .env and configure your environment variables."
        exit 1
    fi

    log_success "Environment file found"
}

pull_images() {
    log_info "Pulling latest Docker images..."

    docker pull $DOCKER_REGISTRY/webapi:latest
    docker pull $DOCKER_REGISTRY/worker:latest
    docker pull $DOCKER_REGISTRY/database:latest

    log_success "Images pulled successfully"
}

build_images() {
    log_info "Building Docker images locally..."

    # Build WebAPI
    docker build -f src/LeadFlowAI.WebAPI/Dockerfile -t $DOCKER_REGISTRY/webapi:latest .

    # Build Worker
    docker build -f src/LeadFlowAI.Worker/Dockerfile -t $DOCKER_REGISTRY/worker:latest .

    # Build Database
    docker build -f database/mongodb/Dockerfile -t $DOCKER_REGISTRY/database:latest .

    log_success "Images built successfully"
}

start_services() {
    log_info "Starting services with Docker Compose..."

    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE up -d

    log_success "Services started successfully"
}

stop_services() {
    log_info "Stopping services..."

    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE down

    log_success "Services stopped successfully"
}

restart_services() {
    log_info "Restarting services..."

    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE restart

    log_success "Services restarted successfully"
}

show_status() {
    log_info "Service status:"
    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE ps
}

show_logs() {
    local service=${1:-""}

    if [ -n "$service" ]; then
        log_info "Showing logs for service: $service"
        docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE logs -f $service
    else
        log_info "Showing logs for all services"
        docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE logs -f
    fi
}

run_migrations() {
    log_info "Running database migrations..."

    # Wait for database to be ready
    log_info "Waiting for database to be ready..."
    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE exec -T postgres sh -c 'while ! pg_isready -U leadflowai -d leadflowai; do sleep 1; done'

    # Run EF Core migrations
    log_info "Applying database migrations..."
    docker run --rm --network leadflowai-network --env-file $ENV_FILE \
        -e ConnectionStrings__DefaultConnection=$DATABASE_URL \
        $DOCKER_REGISTRY/webapi:latest \
        dotnet ef database update --project LeadFlowAI.WebAPI

    log_success "Migrations completed successfully"
}

health_check() {
    log_info "Running health checks..."

    # Wait for services to be healthy
    local max_attempts=30
    local attempt=1

    while [ $attempt -le $max_attempts ]; do
        log_info "Health check attempt $attempt/$max_attempts"

        if curl -f http://localhost:8080/health > /dev/null 2>&1; then
            log_success "Health check passed!"
            return 0
        fi

        sleep 10
        ((attempt++))
    done

    log_error "Health check failed after $max_attempts attempts"
    return 1
}

backup_database() {
    local backup_file="backup_$(date +%Y%m%d_%H%M%S).sql"

    log_info "Creating database backup: $backup_file"

    docker-compose -f docker-compose.prod.yml --env-file $ENV_FILE exec -T postgres \
        pg_dump -U leadflowai -d leadflowai > $backup_file

    log_success "Backup created: $backup_file"
}

show_help() {
    echo "LeadFlowAI Production Deployment Script"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  deploy     - Full deployment (pull images, start services, run migrations, health check)"
    echo "  build      - Build Docker images locally"
    echo "  pull       - Pull latest images from registry"
    echo "  start      - Start all services"
    echo "  stop       - Stop all services"
    echo "  restart    - Restart all services"
    echo "  status     - Show service status"
    echo "  logs       - Show service logs (add service name for specific service)"
    echo "  migrate    - Run database migrations"
    echo "  health     - Run health checks"
    echo "  backup     - Create database backup"
    echo "  help       - Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 deploy"
    echo "  $0 logs webapi"
    echo "  $0 backup"
}

# Main script logic
case "${1:-help}" in
    deploy)
        check_dependencies
        check_env_file
        pull_images
        start_services
        run_migrations
        health_check
        log_success "Deployment completed successfully!"
        ;;
    build)
        check_dependencies
        build_images
        ;;
    pull)
        check_dependencies
        pull_images
        ;;
    start)
        check_dependencies
        check_env_file
        start_services
        ;;
    stop)
        stop_services
        ;;
    restart)
        check_env_file
        restart_services
        ;;
    status)
        show_status
        ;;
    logs)
        show_logs "$2"
        ;;
    migrate)
        check_env_file
        run_migrations
        ;;
    health)
        health_check
        ;;
    backup)
        check_env_file
        backup_database
        ;;
    help|*)
        show_help
        ;;
esac