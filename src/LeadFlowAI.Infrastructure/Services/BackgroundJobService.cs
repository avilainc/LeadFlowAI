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
