using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Lead?> GetByDeduplicationHashAsync(string hash, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Lead?> GetByExternalIdAsync(string externalId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetByStatusAsync(LeadStatus status, int limit, CancellationToken cancellationToken = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<List<Lead>> SearchAsync(Guid tenantId, string? query, LeadStatus? status, LeadSource? source, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default);
}
