namespace LeadFlowAI.Application.DTOs;

public class RDStationWebhookRequest
{
    public string EventType { get; set; } = string.Empty;
    public string EventFamily { get; set; } = string.Empty;
    public RDStationLeadPayload Payload { get; set; } = new();
}

public class RDStationLeadPayload
{
    public string Uuid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? PersonalPhone { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Company { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public string? UtmSource { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmContent { get; set; }
    public string? LatestSourceOrigin { get; set; }
}
