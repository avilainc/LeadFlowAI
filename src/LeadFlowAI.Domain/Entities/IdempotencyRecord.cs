namespace LeadFlowAI.Domain.Entities;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public Guid? LeadId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
