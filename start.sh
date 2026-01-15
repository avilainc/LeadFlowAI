#!/bin/bash

# Script para executar o LeadFlowAI localmente

echo "🚀 Iniciando LeadFlowAI..."

# Verificar se .env existe
if [ ! -f .env ]; then
    echo "⚠️  Arquivo .env não encontrado. Criando a partir do .env.example..."
    cp .env.example .env
    echo "✅ Arquivo .env criado. Por favor, configure suas credenciais e execute novamente."
    exit 0
fi

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker."
    exit 1
fi

echo ""
echo "📦 Subindo serviços com Docker Compose..."
docker-compose up -d

echo ""
echo "⏳ Aguardando serviços iniciarem (15s)..."
sleep 15

echo ""
echo "✅ Serviços iniciados!"
echo ""
echo "📊 URLs disponíveis:"
echo "   API:       http://localhost:5000"
echo "   Swagger:   http://localhost:5000/swagger"
echo "   Hangfire:  http://localhost:5000/hangfire"
echo "   Frontend:  http://localhost:3000"

echo ""
echo "💡 Comandos úteis:"
echo "   Ver logs:      docker-compose logs -f"
echo "   Parar:         docker-compose stop"
echo "   Remover tudo:  docker-compose down -v"

echo ""
echo "✨ LeadFlowAI está rodando!"
