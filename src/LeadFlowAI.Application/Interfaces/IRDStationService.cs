using LeadFlowAI.Domain.Entities;

namespace LeadFlowAI.Application.Interfaces;

public interface IRDStationService
{
    Task<bool> CreateOrUpdateLeadAsync(Lead lead, Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> UpdateLeadTagsAsync(string externalId, List<string> tags, Tenant tenant, CancellationToken cancellationToken = default);
}
