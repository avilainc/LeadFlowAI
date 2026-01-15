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

public class IngestWebFormLeadHandler : IRequestHandler<IngestWebFormLeadCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadEventRepository _eventRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IBackgroundJobService _backgroundJobService;

    public IngestWebFormLeadHandler(
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

    public async Task<Guid> Handle(IngestWebFormLeadCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar tenant
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant == null || !tenant.IsActive)
            throw new Exception($"Tenant '{request.TenantSlug}' não encontrado ou inativo");

        // 2. Normalizar telefone (E.164)
        var phoneNormalized = NormalizePhone(request.Phone);

        // 3. Criar hash de deduplicação
        var dedupHash = CreateDeduplicationHash(phoneNormalized, tenant.Id);

        // 4. Verificar idempotência
        var idempotencyKey = CreateIdempotencyKey(request);
        if (await _idempotencyService.IsProcessedAsync(idempotencyKey, cancellationToken))
        {
            var existingLead = await _leadRepository.GetByDeduplicationHashAsync(dedupHash, tenant.Id, cancellationToken);
            return existingLead?.Id ?? Guid.Empty;
        }

        // 5. Verificar se lead já existe
        var existingLeadByPhone = await _leadRepository.GetByDeduplicationHashAsync(dedupHash, tenant.Id, cancellationToken);
        if (existingLeadByPhone != null)
        {
            // Atualizar lead existente
            existingLeadByPhone.Message = request.Message;
            existingLeadByPhone.Status = LeadStatus.Received;
            existingLeadByPhone.UpdatedAt = DateTime.UtcNow;
            await _leadRepository.UpdateAsync(existingLeadByPhone, cancellationToken);

            await _idempotencyService.MarkAsProcessedAsync(idempotencyKey, existingLeadByPhone.Id, cancellationToken: cancellationToken);
            _backgroundJobService.EnqueueQualifyLead(existingLeadByPhone.Id);

            return existingLeadByPhone.Id;
        }

        // 6. Criar novo lead
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = request.Name,
            Phone = request.Phone,
            PhoneNormalized = phoneNormalized,
            Email = request.Email,
            Company = request.Company,
            City = request.City,
            State = request.State,
            Message = request.Message,
            Source = LeadSource.WebForm,
            SourceUrl = request.SourceUrl,
            UtmSource = request.UtmSource,
            UtmCampaign = request.UtmCampaign,
            UtmMedium = request.UtmMedium,
            UtmContent = request.UtmContent,
            Gclid = request.Gclid,
            Fbclid = request.Fbclid,
            Status = LeadStatus.Received,
            DeduplicationHash = dedupHash,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        await _leadRepository.AddAsync(lead, cancellationToken);

        // 7. Registrar evento
        var leadEvent = new LeadEvent
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            TenantId = tenant.Id,
            EventType = "LEAD_RECEIVED",
            ToStatus = LeadStatus.Received,
            Description = "Lead recebido via formulário web",
            Actor = "system",
            CreatedAt = DateTime.UtcNow
        };
        await _eventRepository.AddAsync(leadEvent, cancellationToken);

        // 8. Marcar como processado
        await _idempotencyService.MarkAsProcessedAsync(idempotencyKey, lead.Id, cancellationToken: cancellationToken);

        // 9. Enfileirar processamento
        _backgroundJobService.EnqueueQualifyLead(lead.Id);

        return lead.Id;
    }

    private string NormalizePhone(string phone)
    {
        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            var parsedNumber = phoneUtil.Parse(phone, "BR");
            return phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        }
        catch
        {
            // Fallback: remover caracteres não numéricos
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }

    private string CreateDeduplicationHash(string phoneNormalized, Guid tenantId)
    {
        var input = $"{phoneNormalized}|{tenantId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    private string CreateIdempotencyKey(IngestWebFormLeadCommand request)
    {
        var input = $"{request.TenantSlug}|{request.Phone}|{request.Email}|{request.Message}|{DateTime.UtcNow:yyyyMMddHH}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
