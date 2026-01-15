using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;
using LeadFlowAI.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using PhoneNumbers;

namespace LeadFlowAI.Application.Handlers;

public class IngestRDStationLeadHandler : IRequestHandler<IngestRDStationLeadCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadEventRepository _eventRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IBackgroundJobService _backgroundJobService;

    public IngestRDStationLeadHandler(
        ITenantRepository tenantRepository,
        ILeadRepository leadRepository,
        ILeadEventRepository eventRepository,
        IIdempotencyService idempotencyService,
        IBackgroundJobService backgroundJobService)
    {
        _tenantRepository = tenantRepository;
        _leadRepository = leadRepository;
        _eventRepository = eventRepository;
        _idempotencyService = idempotencyService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<Guid> Handle(IngestRDStationLeadCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant == null || !tenant.IsActive)
            throw new Exception($"Tenant '{request.TenantSlug}' não encontrado ou inativo");

        // Verificar se já existe pelo ExternalId
        var existingLead = await _leadRepository.GetByExternalIdAsync(request.Payload.Uuid, tenant.Id, cancellationToken);
        if (existingLead != null)
        {
            // Atualizar lead existente
            return existingLead.Id;
        }

        // Normalizar telefone
        var phone = request.Payload.MobilePhone ?? request.Payload.PersonalPhone ?? "";
        var phoneNormalized = NormalizePhone(phone);

        // Criar hash de deduplicação
        var dedupHash = CreateDeduplicationHash(phoneNormalized, tenant.Id);

        // Verificar idempotência
        var idempotencyKey = $"rdstation_{request.Payload.Uuid}";
        if (await _idempotencyService.IsProcessedAsync(idempotencyKey, cancellationToken))
        {
            var existingLeadByHash = await _leadRepository.GetByDeduplicationHashAsync(dedupHash, tenant.Id, cancellationToken);
            return existingLeadByHash?.Id ?? Guid.Empty;
        }

        // Extrair mensagem dos custom fields
        var message = request.Payload.CustomFields.TryGetValue("message", out var msg) ? msg : 
                      request.Payload.CustomFields.TryGetValue("interesse", out var interesse) ? interesse : 
                      "Lead recebido via RD Station";

        // Criar novo lead
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = request.Payload.Name,
            Phone = phone,
            PhoneNormalized = phoneNormalized,
            Email = request.Payload.Email,
            Company = request.Payload.Company,
            City = request.Payload.City,
            State = request.Payload.State,
            Message = message,
            Source = LeadSource.RDStation,
            SourceUrl = request.Payload.LatestSourceOrigin,
            UtmSource = request.Payload.UtmSource,
            UtmCampaign = request.Payload.UtmCampaign,
            UtmMedium = request.Payload.UtmMedium,
            UtmContent = request.Payload.UtmContent,
            Status = LeadStatus.Received,
            DeduplicationHash = dedupHash,
            ExternalId = request.Payload.Uuid,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        await _leadRepository.AddAsync(lead, cancellationToken);

        // Registrar evento
        var leadEvent = new LeadEvent
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            TenantId = tenant.Id,
            EventType = "LEAD_RECEIVED",
            ToStatus = LeadStatus.Received,
            Description = "Lead recebido via RD Station webhook",
            Actor = "rdstation",
            CreatedAt = DateTime.UtcNow
        };
        await _eventRepository.AddAsync(leadEvent, cancellationToken);

        // Marcar como processado
        await _idempotencyService.MarkAsProcessedAsync(idempotencyKey, lead.Id, cancellationToken: cancellationToken);

        // Enfileirar processamento
        _backgroundJobService.EnqueueQualifyLead(lead.Id);

        return lead.Id;
    }

    private string NormalizePhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "";

        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            var parsedNumber = phoneUtil.Parse(phone, "BR");
            return phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        }
        catch
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }

    private string CreateDeduplicationHash(string phoneNormalized, Guid tenantId)
    {
        var input = $"{phoneNormalized}|{tenantId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
