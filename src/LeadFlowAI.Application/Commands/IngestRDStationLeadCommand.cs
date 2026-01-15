using MediatR;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.Application.Commands;

public class IngestRDStationLeadCommand : IRequest<Guid>
{
    public string EventType { get; set; } = string.Empty;
    public RDStationLeadPayload Payload { get; set; } = new();
    public string TenantSlug { get; set; } = string.Empty;
}
