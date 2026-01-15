-- Script inicial do banco de dados LeadFlowAI
-- PostgreSQL 15+

-- Habilitar extensões
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- Para buscas de texto

-- Tabela de Tenants
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    domain VARCHAR(200) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    config JSONB NOT NULL DEFAULT '{}'::jsonb,
    
    -- RD Station Integration
    rd_station_client_id VARCHAR(200),
    rd_station_client_secret VARCHAR(200),
    rd_station_access_token TEXT,
    rd_station_refresh_token TEXT,
    rd_station_token_expires_at TIMESTAMP,
    
    -- WhatsApp Integration
    whats_app_provider VARCHAR(50),
    whats_app_account_id VARCHAR(200),
    whats_app_auth_token TEXT,
    whats_app_from_number VARCHAR(50),
    
    -- Email Integration
    email_provider VARCHAR(50),
    email_api_key TEXT,
    email_from_address VARCHAR(200),
    email_from_name VARCHAR(200),
    
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    
    CONSTRAINT chk_slug_format CHECK (slug ~ '^[a-z0-9-]+$')
);

CREATE INDEX idx_tenants_slug ON tenants(slug);
CREATE INDEX idx_tenants_domain ON tenants(domain);
CREATE INDEX idx_tenants_active ON tenants(is_active) WHERE is_active = true;

-- Tabela de Leads
CREATE TABLE leads (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    
    -- Dados básicos
    name VARCHAR(200) NOT NULL,
    phone VARCHAR(50) NOT NULL,
    phone_normalized VARCHAR(50),
    email VARCHAR(200),
    company VARCHAR(200),
    city VARCHAR(100),
    state VARCHAR(50),
    message TEXT NOT NULL,
    
    -- Origem
    source INTEGER NOT NULL, -- 1=WebForm, 2=RDStation, 3=MetaAds, 4=Manual
    source_url TEXT,
    utm_source VARCHAR(200),
    utm_campaign VARCHAR(200),
    utm_medium VARCHAR(200),
    utm_content VARCHAR(200),
    gclid VARCHAR(200),
    fbclid VARCHAR(200),
    
    -- Estado
    status INTEGER NOT NULL, -- 1=Received, 2=Normalized, 3=Enriched, 4=Qualified, 5=Responded, 6=Handoff, 7=Closed, 8=Failed
    
    -- Deduplicação
    deduplication_hash VARCHAR(100) NOT NULL,
    external_id VARCHAR(200),
    idempotency_key VARCHAR(500),
    
    -- Qualificação LLM
    lead_score INTEGER,
    intent INTEGER, -- 1=Orcamento, 2=Duvida, 3=Suporte, 4=Parceria, 5=Carreira, 6=Outro
    urgency INTEGER, -- 1=Baixa, 2=Media, 3=Alta
    service_match JSONB,
    key_details JSONB,
    missing_questions JSONB,
    risk_flags JSONB,
    recommended_next_step INTEGER, -- 1=Responder, 2=Perguntar, 3=Handoff, 4=Ignorar
    reply_channel INTEGER, -- 1=WhatsApp, 2=Email, 3=Both
    reply_message TEXT,
    handoff_reason TEXT,
    
    -- Resposta
    has_responded BOOLEAN NOT NULL DEFAULT false,
    responded_at TIMESTAMP,
    response_channel VARCHAR(50),
    
    -- Handoff
    is_handed_off BOOLEAN NOT NULL DEFAULT false,
    handed_off_at TIMESTAMP,
    handed_off_by VARCHAR(200),
    
    -- Metadata
    llm_response_raw TEXT,
    retry_count INTEGER NOT NULL DEFAULT 0,
    last_error TEXT,
    
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    
    CONSTRAINT chk_lead_score CHECK (lead_score >= 0 AND lead_score <= 100)
);

CREATE INDEX idx_leads_tenant ON leads(tenant_id);
CREATE INDEX idx_leads_status ON leads(status);
CREATE INDEX idx_leads_created ON leads(created_at DESC);
CREATE INDEX idx_leads_dedup ON leads(deduplication_hash);
CREATE INDEX idx_leads_external ON leads(external_id);
CREATE INDEX idx_leads_tenant_status ON leads(tenant_id, status);
CREATE INDEX idx_leads_tenant_dedup ON leads(tenant_id, deduplication_hash);
CREATE INDEX idx_leads_phone_normalized ON leads(phone_normalized);
CREATE INDEX idx_leads_email ON leads(email);
CREATE INDEX idx_leads_name_trgm ON leads USING gin(name gin_trgm_ops);
CREATE INDEX idx_leads_message_trgm ON leads USING gin(message gin_trgm_ops);

-- Tabela de Eventos de Lead
CREATE TABLE lead_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    lead_id UUID NOT NULL REFERENCES leads(id) ON DELETE CASCADE,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    
    event_type VARCHAR(100) NOT NULL,
    from_status INTEGER,
    to_status INTEGER,
    description TEXT,
    actor VARCHAR(200),
    metadata JSONB,
    
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_lead_events_lead ON lead_events(lead_id);
CREATE INDEX idx_lead_events_created ON lead_events(created_at DESC);
CREATE INDEX idx_lead_events_type ON lead_events(event_type);

-- Tabela de Idempotência
CREATE TABLE idempotency_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    key VARCHAR(500) UNIQUE NOT NULL,
    lead_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_idempotency_key ON idempotency_records(key);
CREATE INDEX idx_idempotency_expires ON idempotency_records(expires_at);

-- Função para limpar registros expirados
CREATE OR REPLACE FUNCTION cleanup_expired_idempotency()
RETURNS void AS $$
BEGIN
    DELETE FROM idempotency_records WHERE expires_at < NOW();
END;
$$ LANGUAGE plpgsql;

-- View para dashboard de métricas
CREATE OR REPLACE VIEW lead_metrics AS
SELECT 
    l.tenant_id,
    COUNT(*) as total_leads,
    COUNT(*) FILTER (WHERE l.status = 1) as received,
    COUNT(*) FILTER (WHERE l.status = 4) as qualified,
    COUNT(*) FILTER (WHERE l.status = 5) as responded,
    COUNT(*) FILTER (WHERE l.status = 6) as handoff,
    COUNT(*) FILTER (WHERE l.status = 7) as closed,
    AVG(l.lead_score) as avg_score,
    COUNT(*) FILTER (WHERE l.has_responded = true) as total_responded,
    COUNT(*) FILTER (WHERE l.source = 1) as from_webform,
    COUNT(*) FILTER (WHERE l.source = 2) as from_rdstation,
    EXTRACT(EPOCH FROM (AVG(l.responded_at - l.created_at))) / 60 as avg_response_time_minutes
FROM leads l
GROUP BY l.tenant_id;

-- Dados de seed (desenvolvimento)
INSERT INTO tenants (id, name, slug, domain, is_active, config) VALUES 
(
    '00000000-0000-0000-0000-000000000001',
    'Empresa Demo',
    'empresa-demo',
    'demo.leadflowai.com',
    true,
    '{
        "playbook": "Somos uma empresa de desenvolvimento de software especializada em soluções web e mobile.",
        "services": ["site", "sistema", "automacao", "consultoria"],
        "regions": ["SP", "RJ", "MG"],
        "minimumPrice": 5000,
        "toneOfVoice": "profissional",
        "scoreThreshold": 50,
        "businessHours": {
            "startTime": "09:00:00",
            "endTime": "18:00:00",
            "workDays": [1, 2, 3, 4, 5]
        },
        "responseTimeMinutes": 15,
        "faqs": [
            {
                "question": "Quanto custa um site?",
                "answer": "O investimento para um site profissional começa em R$ 5.000, variando conforme funcionalidades."
            },
            {
                "question": "Qual o prazo de entrega?",
                "answer": "Projetos simples levam de 4 a 6 semanas. Projetos complexos podem levar de 2 a 4 meses."
            }
        ]
    }'::jsonb
);

COMMENT ON TABLE tenants IS 'Tabela de clientes (multi-tenant)';
COMMENT ON TABLE leads IS 'Tabela de leads capturados';
COMMENT ON TABLE lead_events IS 'Auditoria e timeline de eventos dos leads';
COMMENT ON TABLE idempotency_records IS 'Controle de idempotência para webhooks';
