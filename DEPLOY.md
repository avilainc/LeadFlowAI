# 🚀 Deploy para Produção - LeadFlowAI

## 📋 Pré-requisitos

1. **Conta GitHub** com o repositório
2. **Railway Account** (para backend e bancos)
3. **MongoDB Atlas** (para MongoDB)
4. **Contas de serviços externos:**
   - OpenAI API Key
   - RD Station credentials
   - Twilio (WhatsApp)
   - SendGrid (Email)

---

## 🐳 Docker Images (GitHub Container Registry)

### Configuração Automática

As imagens Docker são construídas e publicadas automaticamente via GitHub Actions sempre que você fizer push para a branch `main`.

**Imagens publicadas:**
- `ghcr.io/avilainc/leadflowai-api:latest`
- `ghcr.io/avilainc/leadflowai-worker:latest`

### Uso Manual

Para publicar manualmente:

```bash
# Login no GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Build e Push API
docker build -t ghcr.io/avilainc/leadflowai-api:latest -f src/LeadFlowAI.WebAPI/Dockerfile .
docker push ghcr.io/avilainc/leadflowai-api:latest

# Build e Push Worker
docker build -t ghcr.io/avilainc/leadflowai-worker:latest -f src/LeadFlowAI.Worker/Dockerfile .
docker push ghcr.io/avilainc/leadflowai-worker:latest
```

---

## 🌐 Frontend (GitHub Pages)

### 1. Habilitar GitHub Pages

1. Vá em **Settings** → **Pages**
2. Source: **GitHub Actions**
3. O deploy acontece automaticamente a cada push na `main`

### 2. Configurar URL da API

O frontend está configurado para usar a variável `VITE_API_URL`.

**URL de produção:** `https://avilainc.github.io/LeadFlowAI`

---

## 🚂 Backend (Railway)

### 1. Criar Projeto Railway

```bash
# Instalar Railway CLI
npm i -g @railway/cli

# Login
railway login

# Criar projeto
railway init
```

### 2. Adicionar PostgreSQL

No dashboard Railway:
1. **New** → **Database** → **PostgreSQL**
2. Copiar a `DATABASE_URL`

### 3. Adicionar Redis

No dashboard Railway:
1. **New** → **Database** → **Redis**
2. Copiar a `REDIS_URL`

### 4. Deploy da API

1. **New** → **GitHub Repo** → Selecione `LeadFlowAI`
2. Configure as variáveis de ambiente:

```env
# Database
DATABASE_URL=${PostgreSQL.DATABASE_URL}
REDIS_URL=${Redis.REDIS_URL}
MONGODB_URI=mongodb+srv://user:pass@cluster.mongodb.net/leadflowai_analytics

# JWT
JWT_SECRET=your-super-secret-jwt-key
JWT_ISSUER=LeadFlowAI
JWT_AUDIENCE=LeadFlowAI

# APIs
OPENAI_API_KEY=sk-...
RDSTATION_CLIENT_ID=...
RDSTATION_CLIENT_SECRET=...
TWILIO_ACCOUNT_SID=...
TWILIO_AUTH_TOKEN=...
TWILIO_WHATSAPP_NUMBER=+...
SENDGRID_API_KEY=SG....

# URLs
FRONTEND_URL=https://avilainc.github.io/LeadFlowAI
CORS_ORIGINS=https://avilainc.github.io

# Config
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

3. Settings → **Build Command**:
```bash
dotnet publish src/LeadFlowAI.WebAPI/LeadFlowAI.WebAPI.csproj -c Release -o out
```

4. Settings → **Start Command**:
```bash
dotnet out/LeadFlowAI.WebAPI.dll
```

### 5. Deploy do Worker

Repita o processo acima, mas usando:
- Build: `dotnet publish src/LeadFlowAI.Worker/LeadFlowAI.Worker.csproj -c Release -o out`
- Start: `dotnet out/LeadFlowAI.Worker.dll`

---

## 🍃 MongoDB (Atlas)

### 1. Criar Cluster

1. Acesse [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. **Create Cluster** (Free tier M0)
3. Escolha região próxima ao Railway

### 2. Configurar Acesso

1. **Database Access** → **Add New User**
   - Username: `leadflowai`
   - Password: (gere uma senha forte)
   - Role: `readWrite`

2. **Network Access** → **Add IP Address**
   - `0.0.0.0/0` (permitir qualquer IP)

### 3. Obter Connection String

1. **Connect** → **Connect your application**
2. Copie a URI:
```
mongodb+srv://leadflowai:<password>@cluster.mongodb.net/leadflowai_analytics?retryWrites=true&w=majority
```

### 4. Inicializar Database

Execute o script `database/init-mongo.js` via **Mongosh**:

```bash
mongosh "mongodb+srv://cluster.mongodb.net/leadflowai_analytics" --username leadflowai
```

---

## ✅ Checklist de Deploy

### GitHub
- [ ] Repositório criado e código enviado
- [ ] GitHub Actions habilitadas
- [ ] GitHub Pages configurado
- [ ] Secrets configurados (se necessário)

### Railway - API
- [ ] Projeto criado
- [ ] PostgreSQL adicionado
- [ ] Redis adicionado
- [ ] Variáveis de ambiente configuradas
- [ ] Deploy funcionando
- [ ] Health check OK (`/health`)

### Railway - Worker
- [ ] Service criado
- [ ] Mesmas variáveis da API configuradas
- [ ] Worker rodando sem erros

### MongoDB Atlas
- [ ] Cluster criado
- [ ] Usuário criado
- [ ] IP liberado
- [ ] Database inicializado
- [ ] Connection string testada

### Frontend
- [ ] Build executando com sucesso
- [ ] Deploy no GitHub Pages OK
- [ ] Conectando com a API
- [ ] CORS configurado corretamente

---

## 🔄 Fluxo de Deploy

```mermaid
graph TD
    A[Push to main] --> B[GitHub Actions]
    B --> C[Build Docker Images]
    C --> D[Push to GHCR]
    B --> E[Build Frontend]
    E --> F[Deploy to GitHub Pages]
    G[Railway] --> H[Pull from GHCR]
    H --> I[Deploy API + Worker]
```

---

## 🔍 Monitoramento

### Logs Railway
```bash
railway logs
```

### Verificar API
```bash
curl https://leadflowai-production.up.railway.app/health
```

### Frontend
Acesse: https://avilainc.github.io/LeadFlowAI

---

## 🆘 Troubleshooting

### API não inicia
- Verifique logs: `railway logs`
- Confirme variáveis de ambiente
- Teste conexão com banco de dados

### Frontend não carrega
- Verifique build no GitHub Actions
- Confirme `VITE_API_URL` correto
- Verifique CORS no backend

### Worker não processa jobs
- Verifique conexão Redis
- Confirme Hangfire configurado
- Verifique logs de erro

---

## 📞 Suporte

Criado por: **Avila Inc**  
Repositório: https://github.com/avilainc/LeadFlowAI
