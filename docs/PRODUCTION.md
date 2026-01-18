# LeadFlowAI - Guia Rápido de Produção

## 🌐 URLs de Produção

- **Frontend**: https://avilainc.github.io/LeadFlowAI
- **API**: https://leadflowai-production.up.railway.app
- **API Docs**: https://leadflowai-production.up.railway.app/swagger

## 🔐 Secrets Necessários

### GitHub Secrets (para Actions)
```
VITE_API_URL=https://leadflowai-production.up.railway.app
```

### Railway Environment Variables

**API & Worker:**
```env
# Database
DATABASE_URL=${PostgreSQL.DATABASE_URL}
REDIS_URL=${Redis.REDIS_URL}
MONGODB_URI=mongodb+srv://...

# Security
JWT_SECRET=...
JWT_ISSUER=LeadFlowAI
JWT_AUDIENCE=LeadFlowAI

# External APIs
OPENAI_API_KEY=sk-...
RDSTATION_CLIENT_ID=...
RDSTATION_CLIENT_SECRET=...
TWILIO_ACCOUNT_SID=...
TWILIO_AUTH_TOKEN=...
TWILIO_WHATSAPP_NUMBER=...
SENDGRID_API_KEY=...

# URLs
FRONTEND_URL=https://avilainc.github.io/LeadFlowAI
CORS_ORIGINS=https://avilainc.github.io

# Config
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

## 🚀 Deploy Rápido

### 1. Frontend (Automático)
```bash
git add .
git commit -m "Deploy frontend"
git push origin main
```

### 2. Docker Images (Automático)
As imagens são construídas automaticamente pelo GitHub Actions quando você faz push.

### 3. Railway (Manual primeira vez)

```bash
# Instalar CLI
npm install -g @railway/cli

# Login
railway login

# Link ao projeto
railway link

# Deploy
railway up
```

## 📊 Monitoramento

### Ver logs da API
```bash
railway logs --service api
```

### Ver logs do Worker
```bash
railway logs --service worker
```

### Testar API
```bash
curl https://leadflowai-production.up.railway.app/health
```

## 🔄 Rollback

### Railway
```bash
railway rollback
```

### Frontend (GitHub Pages)
Faça revert do commit e push novamente.

## 📖 Documentação Completa

Veja [DEPLOY.md](./DEPLOY.md) para instruções detalhadas.
