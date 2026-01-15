using MediatR;

namespace LeadFlowAI.Application.Commands;

public class QualifyLeadCommand : IRequest<bool>
{
    public Guid LeadId { get; set; }
}
