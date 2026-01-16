#!/bin/bash

# Script de deploy para produção
# Usage: ./deploy-prod.sh [api|worker|frontend|all]

set -e

COMPONENT=${1:-all}
REGISTRY="ghcr.io/avilainc"

echo "🚀 Starting production deployment for: $COMPONENT"

deploy_api() {
    echo "📦 Building and deploying API..."
    docker build -t $REGISTRY/leadflowai-api:latest -f src/LeadFlowAI.WebAPI/Dockerfile .
    docker push $REGISTRY/leadflowai-api:latest
    echo "✅ API deployed successfully!"
}

deploy_worker() {
    echo "📦 Building and deploying Worker..."
    docker build -t $REGISTRY/leadflowai-worker:latest -f src/LeadFlowAI.Worker/Dockerfile .
    docker push $REGISTRY/leadflowai-worker:latest
    echo "✅ Worker deployed successfully!"
}

deploy_frontend() {
    echo "📦 Building and deploying Frontend..."
    cd frontend
    npm ci
    npm run build
    echo "✅ Frontend built successfully!"
    echo "💡 Deploy to GitHub Pages will happen automatically on push to main"
    cd ..
}

case $COMPONENT in
    api)
        deploy_api
        ;;
    worker)
        deploy_worker
        ;;
    frontend)
        deploy_frontend
        ;;
    all)
        deploy_api
        deploy_worker
        deploy_frontend
        ;;
    *)
        echo "❌ Invalid component: $COMPONENT"
        echo "Usage: ./deploy-prod.sh [api|worker|frontend|all]"
        exit 1
        ;;
esac

echo ""
echo "✨ Deployment completed!"
echo ""
echo "📍 Next steps:"
echo "1. Check GitHub Actions: https://github.com/avilainc/LeadFlowAI/actions"
echo "2. Monitor Railway: https://railway.app"
echo "3. Verify frontend: https://avilainc.github.io/LeadFlowAI"
