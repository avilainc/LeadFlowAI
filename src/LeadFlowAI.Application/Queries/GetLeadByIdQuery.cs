using MediatR;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.Application.Queries;

public class GetLeadByIdQuery : IRequest<LeadDto?>
{
    public Guid LeadId { get; set; }
    public Guid TenantId { get; set; }
}
