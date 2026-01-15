// Inicialização do banco de dados MongoDB para LeadFlowAI

// Criar database
db = db.getSiblingDB('leadflowai_analytics');

// Criar coleções
db.createCollection('lead_interactions');
db.createCollection('lead_analytics');
db.createCollection('conversation_logs');
db.createCollection('ai_decisions');

// Criar índices para lead_interactions
db.lead_interactions.createIndex({ leadId: 1, timestamp: -1 });
db.lead_interactions.createIndex({ tenantId: 1 });
db.lead_interactions.createIndex({ interactionType: 1 });

// Criar índices para lead_analytics
db.lead_analytics.createIndex({ leadId: 1 }, { unique: true });
db.lead_analytics.createIndex({ tenantId: 1 });
db.lead_analytics.createIndex({ qualificationScore: -1 });
db.lead_analytics.createIndex({ lastInteractionDate: -1 });

// Criar índices para conversation_logs
db.conversation_logs.createIndex({ leadId: 1, timestamp: -1 });
db.conversation_logs.createIndex({ tenantId: 1 });
db.conversation_logs.createIndex({ channel: 1 });

// Criar índices para ai_decisions
db.ai_decisions.createIndex({ leadId: 1, timestamp: -1 });
db.ai_decisions.createIndex({ tenantId: 1 });
db.ai_decisions.createIndex({ modelVersion: 1 });

// Criar usuário de aplicação
db.createUser({
  user: 'leadflowai',
  pwd: 'leadflowai_mongo_password',
  roles: [
    {
      role: 'readWrite',
      db: 'leadflowai_analytics'
    }
  ]
});

print('✅ MongoDB inicializado com sucesso!');
print('📊 Database: leadflowai_analytics');
print('📝 Coleções criadas: lead_interactions, lead_analytics, conversation_logs, ai_decisions');
