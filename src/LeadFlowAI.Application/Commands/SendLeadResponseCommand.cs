using MediatR;

namespace LeadFlowAI.Application.Commands;

public class SendLeadResponseCommand : IRequest<bool>
{
    public Guid LeadId { get; set; }
}
