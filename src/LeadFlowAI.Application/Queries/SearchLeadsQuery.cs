using MediatR;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.Application.Queries;

public class SearchLeadsQuery : IRequest<(List<LeadDto> Leads, int Total)>
{
    public Guid TenantId { get; set; }
    public string? Query { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
