using Microsoft.EntityFrameworkCore;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Infrastructure.Persistence;

namespace LeadFlowAI.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Domain == domain, cancellationToken);
    }

    public async Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.Where(t => t.IsActive).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
