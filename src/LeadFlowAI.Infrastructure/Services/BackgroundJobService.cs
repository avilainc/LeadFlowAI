using Hangfire;
using LeadFlowAI.Application.Interfaces;

namespace LeadFlowAI.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    public void EnqueueQualifyLead(Guid leadId)
    {
        BackgroundJob.Enqueue<IBackgroundJobProcessor>(x => x.ProcessQualifyLeadAsync(leadId, CancellationToken.None));
    }

    public void EnqueueSendResponse(Guid leadId)
    {
        BackgroundJob.Enqueue<IBackgroundJobProcessor>(x => x.ProcessSendResponseAsync(leadId, CancellationToken.None));
    }

    public void EnqueueSyncToRDStation(Guid leadId)
    {
        BackgroundJob.Enqueue<IBackgroundJobProcessor>(x => x.ProcessSyncToRDStationAsync(leadId, CancellationToken.None));
    }
}

// Interface para o processador de jobs
public interface IBackgroundJobProcessor
{
    Task ProcessQualifyLeadAsync(Guid leadId, CancellationToken cancellationToken);
    Task ProcessSendResponseAsync(Guid leadId, CancellationToken cancellationToken);
    Task ProcessSyncToRDStationAsync(Guid leadId, CancellationToken cancellationToken);
}
