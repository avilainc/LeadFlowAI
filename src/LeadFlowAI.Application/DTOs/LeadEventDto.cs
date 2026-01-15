namespace LeadFlowAI.Application.DTOs;

public class LeadEventDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Description { get; set; }
    public string? Actor { get; set; }
    public DateTime CreatedAt { get; set; }
}
