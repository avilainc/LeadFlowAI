namespace LeadFlowAI.Application.DTOs;

public class LLMQualificationResult
{
    public int LeadScore { get; set; }
    public string Intent { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public List<string> ServiceMatch { get; set; } = new();
    public List<string> KeyDetails { get; set; } = new();
    public List<string> MissingQuestions { get; set; } = new();
    public List<string> RiskFlags { get; set; } = new();
    public string RecommendedNextStep { get; set; } = string.Empty;
    public string ReplyChannel { get; set; } = string.Empty;
    public string ReplyMessage { get; set; } = string.Empty;
    public string? HandoffReason { get; set; }
}
