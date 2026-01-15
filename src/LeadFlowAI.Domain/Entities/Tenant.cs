namespace LeadFlowAI.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Configurações
    public TenantConfig Config { get; set; } = new();
    
    // RD Station Integration
    public string? RDStationClientId { get; set; }
    public string? RDStationClientSecret { get; set; }
    public string? RDStationAccessToken { get; set; }
    public string? RDStationRefreshToken { get; set; }
    public DateTime? RDStationTokenExpiresAt { get; set; }
    
    // WhatsApp Integration
    public string? WhatsAppProvider { get; set; }
    public string? WhatsAppAccountId { get; set; }
    public string? WhatsAppAuthToken { get; set; }
    public string? WhatsAppFromNumber { get; set; }
    
    // Email Integration
    public string? EmailProvider { get; set; }
    public string? EmailApiKey { get; set; }
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}

public class TenantConfig
{
    public string Playbook { get; set; } = string.Empty;
    public List<string> Services { get; set; } = new();
    public List<string> Regions { get; set; } = new();
    public decimal? MinimumPrice { get; set; }
    public string ToneOfVoice { get; set; } = "profissional"; // profissional, direto, consultivo
    public int ScoreThreshold { get; set; } = 50;
    public BusinessHours BusinessHours { get; set; } = new();
    public int ResponseTimeMinutes { get; set; } = 15;
    public string? CalendlyLink { get; set; }
    public List<FAQ> FAQs { get; set; } = new();
}

public class BusinessHours
{
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(18, 0, 0);
    public List<DayOfWeek> WorkDays { get; set; } = new() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
}

public class FAQ
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
