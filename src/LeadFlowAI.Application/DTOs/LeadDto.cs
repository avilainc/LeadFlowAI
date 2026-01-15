using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Application.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? LeadScore { get; set; }
    public string? Intent { get; set; }
    public string? Urgency { get; set; }
    public List<string>? ServiceMatch { get; set; }
    public List<string>? RiskFlags { get; set; }
    public bool HasResponded { get; set; }
    public DateTime? RespondedAt { get; set; }
    public bool IsHandedOff { get; set; }
    public DateTime? HandedOffAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
