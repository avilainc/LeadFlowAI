using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Domain.Entities;

public class Lead
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    // Dados básicos
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhoneNormalized { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Message { get; set; } = string.Empty;
    
    // Origem
    public LeadSource Source { get; set; }
    public string? SourceUrl { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmContent { get; set; }
    public string? Gclid { get; set; }
    public string? Fbclid { get; set; }
    
    // Estado
    public LeadStatus Status { get; set; }
    
    // Deduplicação
    public string DeduplicationHash { get; set; } = string.Empty;
    public string? ExternalId { get; set; } // ID do RD Station
    public string? IdempotencyKey { get; set; }
    
    // Qualificação (preenchido pela LLM)
    public int? LeadScore { get; set; }
    public Intent? Intent { get; set; }
    public Urgency? Urgency { get; set; }
    public List<string>? ServiceMatch { get; set; }
    public List<string>? KeyDetails { get; set; }
    public List<string>? MissingQuestions { get; set; }
    public List<string>? RiskFlags { get; set; }
    public RecommendedNextStep? RecommendedNextStep { get; set; }
    public ReplyChannel? ReplyChannel { get; set; }
    public string? ReplyMessage { get; set; }
    public string? HandoffReason { get; set; }
    
    // Resposta
    public bool HasResponded { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseChannel { get; set; }
    
    // Handoff
    public bool IsHandedOff { get; set; }
    public DateTime? HandedOffAt { get; set; }
    public string? HandedOffBy { get; set; }
    
    // Metadata
    public string? LLMResponseRaw { get; set; } // JSON bruto da LLM para auditoria
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<LeadEvent> Events { get; set; } = new List<LeadEvent>();
}
