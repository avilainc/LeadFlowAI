using LeadFlowAI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace LeadFlowAI.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly IConfiguration _configuration;

    public WhatsAppService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var accountSid = _configuration["WhatsApp:AccountSid"] ?? _configuration["TWILIO_ACCOUNT_SID"];
        var authToken = _configuration["WhatsApp:AuthToken"] ?? _configuration["TWILIO_AUTH_TOKEN"];
        
        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
        }
    }

    public async Task<bool> SendMessageAsync(string toNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromNumber = _configuration["WhatsApp:FromNumber"] ?? _configuration["TWILIO_WHATSAPP_NUMBER"];
            
            if (string.IsNullOrEmpty(fromNumber))
            {
                throw new Exception("WhatsApp FromNumber não configurado");
            }

            // Garantir formato whatsapp:+number
            if (!toNumber.StartsWith("whatsapp:"))
            {
                toNumber = $"whatsapp:{toNumber}";
            }

            if (!fromNumber.StartsWith("whatsapp:"))
            {
                fromNumber = $"whatsapp:{fromNumber}";
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(toNumber)
            );

            return messageResource.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            // Log error (pode integrar com ILogger aqui)
            Console.WriteLine($"Erro ao enviar WhatsApp: {ex.Message}");
            return false;
        }
    }
}
