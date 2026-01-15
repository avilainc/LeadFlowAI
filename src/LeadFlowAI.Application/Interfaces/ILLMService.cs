namespace LeadFlowAI.Application.Interfaces;

public interface ILLMService
{
    Task<string> QualifyLeadAsync(Guid leadId, Guid tenantId, CancellationToken cancellationToken = default);
}
