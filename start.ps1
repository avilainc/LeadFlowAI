# Script para executar o LeadFlowAI localmente

Write-Host "🚀 Iniciando LeadFlowAI..." -ForegroundColor Green

# Verificar se .env existe
if (-not (Test-Path ".env")) {
    Write-Host "⚠️  Arquivo .env não encontrado. Criando a partir do .env.example..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "✅ Arquivo .env criado. Por favor, configure suas credenciais e execute novamente." -ForegroundColor Green
    exit
}

# Verificar se Docker está rodando
$dockerRunning = docker info 2>&1 | Select-String "Server Version"
if (-not $dockerRunning) {
    Write-Host "❌ Docker não está rodando. Por favor, inicie o Docker Desktop." -ForegroundColor Red
    exit 1
}

Write-Host "`n📦 Subindo serviços com Docker Compose..." -ForegroundColor Cyan
docker-compose up -d

Write-Host "`n⏳ Aguardando serviços iniciarem (15s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`n✅ Serviços iniciados!" -ForegroundColor Green
Write-Host "`n📊 URLs disponíveis:" -ForegroundColor Cyan
Write-Host "   API:       http://localhost:5000" -ForegroundColor White
Write-Host "   Swagger:   http://localhost:5000/swagger" -ForegroundColor White
Write-Host "   Hangfire:  http://localhost:5000/hangfire" -ForegroundColor White
Write-Host "   Frontend:  http://localhost:3000" -ForegroundColor White

Write-Host "`n💡 Comandos úteis:" -ForegroundColor Cyan
Write-Host "   Ver logs:      docker-compose logs -f" -ForegroundColor White
Write-Host "   Parar:         docker-compose stop" -ForegroundColor White
Write-Host "   Remover tudo:  docker-compose down -v" -ForegroundColor White

Write-Host "`n✨ LeadFlowAI está rodando!" -ForegroundColor Green
