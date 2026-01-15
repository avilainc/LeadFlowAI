using Microsoft.EntityFrameworkCore;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Infrastructure.Persistence;

namespace LeadFlowAI.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly ApplicationDbContext _context;

    public IdempotencyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default)
    {
        // Limpar registros expirados
        await CleanupExpiredRecordsAsync(cancellationToken);

        return await _context.IdempotencyRecords
            .AnyAsync(r => r.Key == key && r.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(string key, Guid? leadId, int expirationHours = 24, CancellationToken cancellationToken = default)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            Key = key,
            LeadId = leadId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        };

        await _context.IdempotencyRecords.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task CleanupExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        var expiredRecords = await _context.IdempotencyRecords
            .Where(r => r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredRecords.Any())
        {
            _context.IdempotencyRecords.RemoveRange(expiredRecords);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
