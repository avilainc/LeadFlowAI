using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;
using LeadFlowAI.Domain.Interfaces;

namespace LeadFlowAI.Application.Handlers;

public class SendLeadResponseHandler : IRequestHandler<SendLeadResponseCommand, bool>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILeadEventRepository _eventRepository;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IEmailService _emailService;

    public SendLeadResponseHandler(
        ILeadRepository leadRepository,
        ITenantRepository tenantRepository,
        ILeadEventRepository eventRepository,
        IWhatsAppService whatsAppService,
        IEmailService emailService)
    {
        _leadRepository = leadRepository;
        _tenantRepository = tenantRepository;
        _eventRepository = eventRepository;
        _whatsAppService = whatsAppService;
        _emailService = emailService;
    }

    public async Task<bool> Handle(SendLeadResponseCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null || lead.HasResponded || string.IsNullOrEmpty(lead.ReplyMessage))
            return false;

        var tenant = await _tenantRepository.GetByIdAsync(lead.TenantId, cancellationToken);
        if (tenant == null) return false;

        try
        {
            // Verificar horário comercial
            var now = DateTime.Now;
            var isBusinessHours = IsWithinBusinessHours(now, tenant.Config.BusinessHours);

            if (!isBusinessHours)
            {
                // Enviar mensagem de "recebimento" fora do horário
                var afterHoursMessage = $"Olá {lead.Name}! Recebemos sua mensagem. Nossa equipe retornará em breve no horário comercial. Obrigado!";
                await SendMessageAsync(lead, tenant, afterHoursMessage, cancellationToken);
                
                // Reagendar para próximo horário comercial
                return true;
            }

            // Enviar mensagem
            bool sent = await SendMessageAsync(lead, tenant, lead.ReplyMessage, cancellationToken);

            if (sent)
            {
                lead.HasResponded = true;
                lead.RespondedAt = DateTime.UtcNow;
                lead.Status = LeadStatus.Responded;
                lead.ResponseChannel = lead.ReplyChannel.ToString();
                lead.UpdatedAt = DateTime.UtcNow;
                await _leadRepository.UpdateAsync(lead, cancellationToken);

                await AddEventAsync(lead.Id, lead.TenantId, "RESPONSE_SENT", LeadStatus.Qualified, LeadStatus.Responded,
                    $"Resposta enviada via {lead.ReplyChannel}", cancellationToken);

                // Se recomendado handoff, fazer transição
                if (lead.RecommendedNextStep == RecommendedNextStep.Handoff)
                {
                    lead.Status = LeadStatus.Handoff;
                    lead.IsHandedOff = true;
                    lead.HandedOffAt = DateTime.UtcNow;
                    await _leadRepository.UpdateAsync(lead, cancellationToken);

                    await AddEventAsync(lead.Id, lead.TenantId, "AUTO_HANDOFF", LeadStatus.Responded, LeadStatus.Handoff,
                        lead.HandoffReason ?? "Encaminhado conforme recomendação da LLM", cancellationToken);
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            lead.Status = LeadStatus.Failed;
            lead.LastError = $"Erro ao enviar resposta: {ex.Message}";
            lead.RetryCount++;
            lead.UpdatedAt = DateTime.UtcNow;
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            await AddEventAsync(lead.Id, lead.TenantId, "RESPONSE_FAILED", null, LeadStatus.Failed,
                $"Erro ao enviar resposta: {ex.Message}", cancellationToken);

            throw;
        }
    }

    private async Task<bool> SendMessageAsync(Lead lead, Tenant tenant, string message, CancellationToken cancellationToken)
    {
        bool sent = false;

        if (lead.ReplyChannel == ReplyChannel.WhatsApp || lead.ReplyChannel == ReplyChannel.Both)
        {
            if (!string.IsNullOrEmpty(lead.PhoneNormalized))
            {
                sent = await _whatsAppService.SendMessageAsync(lead.PhoneNormalized, message, cancellationToken);
            }
        }

        if (lead.ReplyChannel == ReplyChannel.Email || lead.ReplyChannel == ReplyChannel.Both)
        {
            if (!string.IsNullOrEmpty(lead.Email))
            {
                var subject = $"Re: {lead.Message.Substring(0, Math.Min(50, lead.Message.Length))}...";
                sent = await _emailService.SendEmailAsync(lead.Email, subject, message, cancellationToken) || sent;
            }
        }

        return sent;
    }

    private bool IsWithinBusinessHours(DateTime now, BusinessHours hours)
    {
        if (!hours.WorkDays.Contains(now.DayOfWeek))
            return false;

        var currentTime = now.TimeOfDay;
        return currentTime >= hours.StartTime && currentTime <= hours.EndTime;
    }

    private async Task AddEventAsync(Guid leadId, Guid tenantId, string eventType, LeadStatus? fromStatus, LeadStatus? toStatus, string description, CancellationToken cancellationToken)
    {
        var evt = new Domain.Entities.LeadEvent
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            TenantId = tenantId,
            EventType = eventType,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Description = description,
            Actor = "system",
            CreatedAt = DateTime.UtcNow
        };
        await _eventRepository.AddAsync(evt, cancellationToken);
    }
}
