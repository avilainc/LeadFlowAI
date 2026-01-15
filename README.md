# LeadFlowAI

**Sistema SaaS Multi-Tenant de Automação de Qualificação de Leads com IA**

LeadFlowAI é uma plataforma corporativa que automatiza o processo de captura, qualificação e resposta inicial a leads usando LLM (Large Language Models) como agente SDR.

## 🎯 Objetivo

Quando um lead entra via:
- **Formulário Web** (primeiro canal)
- **Meta Ads → RD Station** (segundo canal)

O sistema:
1. ✅ Captura e normaliza o lead
2. 🤖 Analisa com LLM (intenção, urgência, fit, sentimento, risco)
3. 💬 Responde automaticamente (WhatsApp/Email) com linguagem adequada
4. 📊 Sincroniza com RD Station
5. 👤 Encaminha para humano quando necessário (handoff)

## 🏗️ Arquitetura

### Clean Architecture em .NET 8

```
LeadFlowAI/
├── src/
│   ├── LeadFlowAI.Domain/          # Entidades, Enums, Interfaces
│   ├── LeadFlowAI.Application/     # DTOs, Commands, Queries, Handlers (MediatR)
│   ├── LeadFlowAI.Infrastructure/  # EF Core, Repositórios, Integrações Externas
│   ├── LeadFlowAI.WebAPI/          # Controllers, Middlewares, Auth JWT
│   └── LeadFlowAI.Worker/          # Background Jobs (Hangfire)
├── frontend/                       # React + TypeScript + TailwindCSS
├── database/                       # Scripts SQL
└── docker-compose.yml
```

### Stack Tecnológica

**Backend:**
- .NET 8 (C#)
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 15
- Hangfire (fila de jobs)
- Redis (cache)
- MediatR (CQRS)

**Integrações:**
- OpenAI GPT-4 (LLM)
- Twilio (WhatsApp)
- SendGrid (Email)
- RD Station Marketing

**Frontend:**
- React 18
- TypeScript
- TailwindCSS
- React Query
- React Router

**Infraestrutura:**
- Docker & Docker Compose
- Nginx (reverse proxy no frontend)

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://www.docker.com/)
- [PostgreSQL 15+](https://www.postgresql.org/) (ou usar via Docker)

## 🚀 Setup Local

### 1. Clonar e Configurar Variáveis

```bash
# Copiar arquivo de exemplo
cp .env.example .env

# Editar .env com suas credenciais
# Obrigatórias:
# - OPENAI_API_KEY
# - JWT_SECRET
# Opcionais (para produção):
# - TWILIO_*
# - SENDGRID_API_KEY
# - RDSTATION_*
```

### 2. Iniciar com Docker Compose (Recomendado)

```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs
docker-compose logs -f

# Acessar:
# - API: http://localhost:5000
# - Frontend: http://localhost:3000
# - Hangfire Dashboard: http://localhost:5000/hangfire
```

### 3. Setup Manual (Desenvolvimento)

#### Backend

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
cd src/LeadFlowAI.WebAPI
dotnet ef database update

# Executar API
dotnet run --project src/LeadFlowAI.WebAPI

# Em outro terminal, executar Worker
dotnet run --project src/LeadFlowAI.Worker
```

#### Frontend

```bash
cd frontend

# Instalar dependências
npm install

# Executar em modo desenvolvimento
npm run dev

# Build para produção
npm run build
```

## 📊 Banco de Dados

### Executar Script Inicial

```bash
# Via Docker
docker exec -i leadflowai-postgres psql -U leadflowai -d leadflowai < database/init.sql

# Via psql local
psql -U leadflowai -d leadflowai -f database/init.sql
```

### Migrations (EF Core)

```bash
cd src/LeadFlowAI.Infrastructure

# Criar nova migration
dotnet ef migrations add NomeDaMigration --startup-project ../LeadFlowAI.WebAPI

# Aplicar migrations
dotnet ef database update --startup-project ../LeadFlowAI.WebAPI
```

## 🔌 Endpoints da API

### Ingestão de Leads

**POST** `/api/leads/ingest/webform`
```json
{
  "name": "João Silva",
  "phone": "+5511999999999",
  "email": "joao@example.com",
  "company": "Empresa XYZ",
  "city": "São Paulo",
  "state": "SP",
  "message": "Gostaria de um orçamento para site institucional",
  "utmSource": "google",
  "utmCampaign": "institucional-2024",
  "sourceUrl": "https://exemplo.com/contato",
  "tenantSlug": "empresa-demo"
}
```

**POST** `/api/leads/ingest/rdstation/webhook?tenantSlug=empresa-demo`
```json
{
  "eventType": "CONVERSION",
  "payload": {
    "uuid": "abc123",
    "name": "Maria Santos",
    "email": "maria@example.com",
    "mobilePhone": "+5511988888888",
    "city": "Rio de Janeiro",
    "state": "RJ"
  }
}
```

### Consulta de Leads (Requer Auth)

**GET** `/api/leads/search?query=joão&status=Qualified&page=1&pageSize=20`

**GET** `/api/leads/{id}`

**GET** `/api/leads/{id}/events`

**POST** `/api/leads/{id}/handoff`

## 🤖 Como Funciona a Qualificação LLM

### Prompt do Sistema (SDR Agent)

O agente recebe:
- **Dados do lead** (nome, telefone, mensagem, origem)
- **Playbook do tenant** (serviços, regiões, preço mínimo, tom de voz)
- **FAQs**
- **Regras de compliance** (LGPD, não pedir dados sensíveis)

### Saída Estruturada (JSON)

```json
{
  "lead_score": 85,
  "intent": "orcamento",
  "urgency": "alta",
  "service_match": ["site", "sistema"],
  "key_details": ["Precisa de site institucional", "Orçamento aproximado de R$ 10k"],
  "missing_questions": ["Qual o prazo desejado?"],
  "risk_flags": [],
  "recommended_next_step": "responder",
  "reply_channel": "whatsapp",
  "reply_message": "Olá João! Obrigado pelo contato. Vou te enviar uma proposta personalizada para o site institucional. Qual seria o prazo ideal para você?",
  "handoff_reason": null
}
```

### Guardrails (Regras Determinísticas)

1. **Dados sensíveis detectados** → Responder com aviso + handoff automático
2. **Score < 50 + intenção "carreira"** → Resposta curta + fechar
3. **Urgência alta + fit alto** → Responder + propor agendamento

## 🔐 Autenticação

A API usa JWT Bearer Token. Para obter token (implementar endpoint de login):

```bash
# Header
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📱 Dashboard Administrativo

Acesse http://localhost:3000

**Funcionalidades:**
- ✅ Lista de leads com filtros (status, origem, data)
- ✅ Detalhes do lead (dados, score, qualificação LLM)
- ✅ Timeline de eventos (auditoria completa)
- ✅ Botão "Assumir conversa" (handoff)
- 🔲 Configurações do tenant (WIP)
- 🔲 Dashboard de métricas (WIP)

## 🔄 Pipeline do Lead (Estados)

```
RECEIVED → NORMALIZED → ENRICHED → QUALIFIED → RESPONDED → HANDOFF/CLOSED
                                                     ↓
                                                  FAILED (com retry)
```

## 🛡️ Segurança e Compliance (LGPD)

- ✅ Criptografia em trânsito (HTTPS obrigatório em produção)
- ✅ Masking de PII em logs
- ✅ Idempotência de webhooks (evita duplicatas)
- ✅ Deduplicação por telefone normalizado (E.164)
- ✅ Auditoria completa (tabela `lead_events`)
- ✅ Consentimento no formulário (checkbox configurável)

## 📊 Observabilidade

### Hangfire Dashboard

Acesse http://localhost:5000/hangfire

- Visualizar jobs enfileirados
- Retry de jobs falhos
- Métricas de processamento

### Logs

Logs são gravados em:
- Console (desenvolvimento)
- Arquivos `logs/leadflowai-YYYY-MM-DD.log`

## 🧪 Testes (TODO)

```bash
# Testes unitários
dotnet test

# Testes de integração
dotnet test --filter Category=Integration
```

## 📦 Deploy

### Docker (Produção)

```bash
# Build de imagens
docker-compose build

# Subir em produção
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Variáveis de Ambiente Críticas

```bash
# Obrigatórias para produção:
JWT_SECRET=<mínimo-32-caracteres>
OPENAI_API_KEY=sk-...
DB_PASSWORD=<senha-forte>

# Recomendadas:
TWILIO_ACCOUNT_SID=...
TWILIO_AUTH_TOKEN=...
SENDGRID_API_KEY=...
RDSTATION_CLIENT_ID=...
RDSTATION_CLIENT_SECRET=...
```

## 📚 Documentação Adicional

- [Swagger/OpenAPI](http://localhost:5000/swagger) - Documentação interativa da API
- [Hangfire](http://localhost:5000/hangfire) - Dashboard de jobs

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📝 Licença

Este projeto é proprietário. Todos os direitos reservados.

## 🆘 Suporte

Para dúvidas e suporte:
- Email: contato@leadflowai.com
- Documentação: https://docs.leadflowai.com

---

**LeadFlowAI** - Automatize a qualificação de leads com inteligência artificial 🚀
