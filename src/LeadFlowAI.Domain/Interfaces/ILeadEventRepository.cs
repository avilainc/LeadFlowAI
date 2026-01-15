using LeadFlowAI.Domain.Entities;

namespace LeadFlowAI.Domain.Interfaces;

public interface ILeadEventRepository
{
    Task AddAsync(LeadEvent leadEvent, CancellationToken cancellationToken = default);
    Task<List<LeadEvent>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);
}
