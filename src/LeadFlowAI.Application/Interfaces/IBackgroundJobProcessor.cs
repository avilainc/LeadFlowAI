namespace LeadFlowAI.Application.Interfaces;

/// <summary>
/// Interface para processamento assíncrono de jobs de background via Hangfire
/// </summary>
public interface IBackgroundJobProcessor
{
    Task ProcessQualifyLeadAsync(Guid leadId, CancellationToken cancellationToken);
    Task ProcessSendResponseAsync(Guid leadId, CancellationToken cancellationToken);
    Task ProcessSyncToRDStationAsync(Guid leadId, CancellationToken cancellationToken);
}
