using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Domain.Entities;

public class LeadEvent
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Guid TenantId { get; set; }
    
    public string EventType { get; set; } = string.Empty; // STATUS_CHANGED, MESSAGE_SENT, LLM_QUALIFIED, HANDOFF, etc
    public LeadStatus? FromStatus { get; set; }
    public LeadStatus? ToStatus { get; set; }
    
    public string? Description { get; set; }
    public string? Actor { get; set; } // "system", "llm", "user@email.com"
    public string? Metadata { get; set; } // JSON adicional
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Lead Lead { get; set; } = null!;
}
