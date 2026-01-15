using MediatR;

namespace LeadFlowAI.Application.Commands;

public class IngestWebFormLeadCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UtmSource { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmContent { get; set; }
    public string? Gclid { get; set; }
    public string? Fbclid { get; set; }
    public string? SourceUrl { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
}
