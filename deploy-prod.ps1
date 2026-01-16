# Script de deploy para produção (PowerShell)
# Usage: .\deploy-prod.ps1 [-Component "api"|"worker"|"frontend"|"all"]

param(
    [Parameter(Position=0)]
    [ValidateSet("api", "worker", "frontend", "all")]
    [string]$Component = "all"
)

$ErrorActionPreference = "Stop"
$REGISTRY = "ghcr.io/avilainc"

Write-Host "🚀 Starting production deployment for: $Component" -ForegroundColor Cyan

function Deploy-API {
    Write-Host "📦 Building and deploying API..." -ForegroundColor Yellow
    docker build -t "$REGISTRY/leadflowai-api:latest" -f src/LeadFlowAI.WebAPI/Dockerfile .
    docker push "$REGISTRY/leadflowai-api:latest"
    Write-Host "✅ API deployed successfully!" -ForegroundColor Green
}

function Deploy-Worker {
    Write-Host "📦 Building and deploying Worker..." -ForegroundColor Yellow
    docker build -t "$REGISTRY/leadflowai-worker:latest" -f src/LeadFlowAI.Worker/Dockerfile .
    docker push "$REGISTRY/leadflowai-worker:latest"
    Write-Host "✅ Worker deployed successfully!" -ForegroundColor Green
}

function Deploy-Frontend {
    Write-Host "📦 Building and deploying Frontend..." -ForegroundColor Yellow
    Push-Location frontend
    npm ci
    npm run build
    Write-Host "✅ Frontend built successfully!" -ForegroundColor Green
    Write-Host "💡 Deploy to GitHub Pages will happen automatically on push to main" -ForegroundColor Cyan
    Pop-Location
}

switch ($Component) {
    "api" {
        Deploy-API
    }
    "worker" {
        Deploy-Worker
    }
    "frontend" {
        Deploy-Frontend
    }
    "all" {
        Deploy-API
        Deploy-Worker
        Deploy-Frontend
    }
}

Write-Host ""
Write-Host "✨ Deployment completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Next steps:" -ForegroundColor Cyan
Write-Host "1. Check GitHub Actions: https://github.com/avilainc/LeadFlowAI/actions"
Write-Host "2. Monitor Railway: https://railway.app"
Write-Host "3. Verify frontend: https://avilainc.github.io/LeadFlowAI"
