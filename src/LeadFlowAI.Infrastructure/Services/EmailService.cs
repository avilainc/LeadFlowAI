using LeadFlowAI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LeadFlowAI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SendGridClient _client;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var apiKey = _configuration["Email:ApiKey"] ?? _configuration["SENDGRID_API_KEY"];
        _client = new SendGridClient(apiKey);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromEmail = _configuration["Email:FromEmail"] ?? _configuration["EMAIL_FROM"] ?? "noreply@leadflowai.com";
            var fromName = _configuration["Email:FromName"] ?? _configuration["EMAIL_FROM_NAME"] ?? "LeadFlowAI";

            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);
            var plainTextContent = body;
            var htmlContent = $"<p>{body.Replace("\n", "<br/>")}</p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            return false;
        }
    }
}
