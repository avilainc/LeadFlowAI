using MediatR;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.Application.Queries;

public class GetLeadEventsQuery : IRequest<List<LeadEventDto>>
{
    public Guid LeadId { get; set; }
}
