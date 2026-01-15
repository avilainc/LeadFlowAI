using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Domain.Enums;
using LeadFlowAI.Domain.Interfaces;
using System.Text.Json;

namespace LeadFlowAI.Application.Handlers;

public class QualifyLeadHandler : IRequestHandler<QualifyLeadCommand, bool>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILeadEventRepository _eventRepository;
    private readonly ILLMService _llmService;
    private readonly IBackgroundJobService _backgroundJobService;

    public QualifyLeadHandler(
        ILeadRepository leadRepository,
        ITenantRepository tenantRepository,
        ILeadEventRepository eventRepository,
        ILLMService llmService,
        IBackgroundJobService backgroundJobService)
    {
        _leadRepository = leadRepository;
        _tenantRepository = tenantRepository;
        _eventRepository = eventRepository;
        _llmService = llmService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<bool> Handle(QualifyLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null) return false;

        var tenant = await _tenantRepository.GetByIdAsync(lead.TenantId, cancellationToken);
        if (tenant == null) return false;

        try
        {
            // 1. Atualizar status para NORMALIZED
            lead.Status = LeadStatus.Normalized;
            lead.UpdatedAt = DateTime.UtcNow;
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            await AddEventAsync(lead.Id, lead.TenantId, "STATUS_CHANGED", LeadStatus.Received, LeadStatus.Normalized, "Lead normalizado", cancellationToken);

            // 2. Chamar LLM
            var llmResponse = await _llmService.QualifyLeadAsync(lead.Id, tenant.Id, cancellationToken);
            
            // 3. Parse do JSON
            var qualification = JsonSerializer.Deserialize<LLMQualificationResult>(llmResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (qualification == null)
                throw new Exception("Resposta da LLM inválida");

            // 4. Atualizar lead com qualificação
            lead.LeadScore = qualification.LeadScore;
            lead.Intent = Enum.Parse<Intent>(qualification.Intent, true);
            lead.Urgency = Enum.Parse<Urgency>(qualification.Urgency, true);
            lead.ServiceMatch = qualification.ServiceMatch;
            lead.KeyDetails = qualification.KeyDetails;
            lead.MissingQuestions = qualification.MissingQuestions;
            lead.RiskFlags = qualification.RiskFlags;
            lead.RecommendedNextStep = Enum.Parse<RecommendedNextStep>(qualification.RecommendedNextStep, true);
            lead.ReplyChannel = Enum.Parse<ReplyChannel>(qualification.ReplyChannel, true);
            lead.ReplyMessage = qualification.ReplyMessage;
            lead.HandoffReason = qualification.HandoffReason;
            lead.LLMResponseRaw = llmResponse;
            lead.Status = LeadStatus.Qualified;
            lead.UpdatedAt = DateTime.UtcNow;

            await _leadRepository.UpdateAsync(lead, cancellationToken);

            await AddEventAsync(lead.Id, lead.TenantId, "LLM_QUALIFIED", LeadStatus.Normalized, LeadStatus.Qualified, 
                $"Lead qualificado pela LLM. Score: {lead.LeadScore}, Intent: {lead.Intent}", cancellationToken);

            // 5. Aplicar regras determinísticas (guardrails)
            if (qualification.RiskFlags.Contains("dados_sensiveis"))
            {
                lead.Status = LeadStatus.Handoff;
                lead.IsHandedOff = true;
                lead.HandedOffAt = DateTime.UtcNow;
                lead.HandoffReason = "Dados sensíveis detectados";
                await _leadRepository.UpdateAsync(lead, cancellationToken);

                await AddEventAsync(lead.Id, lead.TenantId, "AUTO_HANDOFF", LeadStatus.Qualified, LeadStatus.Handoff, 
                    "Lead encaminhado automaticamente por detectar dados sensíveis", cancellationToken);
            }
            else if (lead.LeadScore < tenant.Config.ScoreThreshold && (lead.Intent == Intent.Carreira || qualification.RiskFlags.Contains("spam_suspeito")))
            {
                lead.Status = LeadStatus.Closed;
                await _leadRepository.UpdateAsync(lead, cancellationToken);

                await AddEventAsync(lead.Id, lead.TenantId, "AUTO_CLOSED", LeadStatus.Qualified, LeadStatus.Closed, 
                    "Lead fechado automaticamente por baixo score e intenção inadequada", cancellationToken);
            }
            else
            {
                // 6. Enfileirar resposta automática
                _backgroundJobService.EnqueueSendResponse(lead.Id);

                // 7. Enfileirar sync com RD Station
                _backgroundJobService.EnqueueSyncToRDStation(lead.Id);
            }

            return true;
        }
        catch (Exception ex)
        {
            lead.Status = LeadStatus.Failed;
            lead.LastError = ex.Message;
            lead.RetryCount++;
            lead.UpdatedAt = DateTime.UtcNow;
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            await AddEventAsync(lead.Id, lead.TenantId, "QUALIFICATION_FAILED", null, LeadStatus.Failed, 
                $"Erro na qualificação: {ex.Message}", cancellationToken);

            throw;
        }
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
