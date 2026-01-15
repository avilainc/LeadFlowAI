using Microsoft.EntityFrameworkCore;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Infrastructure.Persistence;

namespace LeadFlowAI.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly ApplicationDbContext _context;

    public LeadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Lead?> GetByDeduplicationHashAsync(string hash, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.DeduplicationHash == hash && l.TenantId == tenantId, cancellationToken);
    }

    public async Task<Lead?> GetByExternalIdAsync(string externalId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.ExternalId == externalId && l.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<Lead>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.TenantId == tenantId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetByStatusAsync(LeadStatus status, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.Status == status)
            .OrderBy(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads.CountAsync(l => l.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        lead.UpdatedAt = DateTime.UtcNow;
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Lead>> SearchAsync(Guid tenantId, string? query, LeadStatus? status, LeadSource? source, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Leads.Where(l => l.TenantId == tenantId);

        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable.Where(l => 
                l.Name.Contains(query) || 
                l.Email!.Contains(query) || 
                l.Phone.Contains(query) ||
                l.Company!.Contains(query));
        }

        if (status.HasValue)
            queryable = queryable.Where(l => l.Status == status.Value);

        if (source.HasValue)
            queryable = queryable.Where(l => l.Source == source.Value);

        if (startDate.HasValue)
            queryable = queryable.Where(l => l.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            queryable = queryable.Where(l => l.CreatedAt <= endDate.Value);

        return await queryable
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
