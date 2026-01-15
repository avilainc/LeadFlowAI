using LeadFlowAI.Domain.Entities;

namespace LeadFlowAI.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
