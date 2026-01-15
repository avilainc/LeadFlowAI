namespace LeadFlowAI.Application.Interfaces;

public interface IBackgroundJobService
{
    void EnqueueQualifyLead(Guid leadId);
    void EnqueueSendResponse(Guid leadId);
    void EnqueueSyncToRDStation(Guid leadId);
}
