namespace LeadFlowAI.Domain.Interfaces;

public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string key, Guid? leadId, int expirationHours = 24, CancellationToken cancellationToken = default);
}
