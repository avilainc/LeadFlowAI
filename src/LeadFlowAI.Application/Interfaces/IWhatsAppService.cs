namespace LeadFlowAI.Application.Interfaces;

public interface IWhatsAppService
{
    Task<bool> SendMessageAsync(string toNumber, string message, CancellationToken cancellationToken = default);
}
