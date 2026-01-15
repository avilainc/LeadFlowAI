using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace LeadFlowAI.Worker;

public class BackgroundJobProcessor : IBackgroundJobProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobProcessor> _logger;

    public BackgroundJobProcessor(IServiceProvider serviceProvider, ILogger<BackgroundJobProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessQualifyLeadAsync(Guid leadId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var leadRepo = scope.ServiceProvider.GetRequiredService<ILeadRepository>();

        _logger.LogInformation("Iniciando qualificação do lead {LeadId}", leadId);

        try
        {
            var command = new QualifyLeadCommand { LeadId = leadId };
            await mediator.Send(command, cancellationToken);

            _logger.LogInformation("Lead {LeadId} qualificado com sucesso", leadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao qualificar lead {LeadId}", leadId);

            var lead = await leadRepo.GetByIdAsync(leadId, cancellationToken);
            if (lead != null && lead.RetryCount >= 3)
            {
                _logger.LogWarning("Lead {LeadId} excedeu número de tentativas, marcando como FAILED", leadId);
            }

            throw;
        }
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessSendResponseAsync(Guid leadId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        _logger.LogInformation("Enviando resposta para o lead {LeadId}", leadId);

        try
        {
            var command = new SendLeadResponseCommand { LeadId = leadId };
            await mediator.Send(command, cancellationToken);

            _logger.LogInformation("Resposta enviada com sucesso para o lead {LeadId}", leadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar resposta para o lead {LeadId}", leadId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
    public async Task ProcessSyncToRDStationAsync(Guid leadId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var leadRepo = scope.ServiceProvider.GetRequiredService<ILeadRepository>();
        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var rdService = scope.ServiceProvider.GetRequiredService<IRDStationService>();

        _logger.LogInformation("Sincronizando lead {LeadId} com RD Station", leadId);

        try
        {
            var lead = await leadRepo.GetByIdAsync(leadId, cancellationToken);
            if (lead == null)
            {
                _logger.LogWarning("Lead {LeadId} não encontrado", leadId);
                return;
            }

            var tenant = await tenantRepo.GetByIdAsync(lead.TenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} não encontrado", lead.TenantId);
                return;
            }

            if (string.IsNullOrEmpty(tenant.RDStationClientId))
            {
                _logger.LogInformation("Tenant {TenantId} não possui integração com RD Station configurada", tenant.Id);
                return;
            }

            await rdService.CreateOrUpdateLeadAsync(lead, tenant, cancellationToken);

            _logger.LogInformation("Lead {LeadId} sincronizado com RD Station com sucesso", leadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar lead {LeadId} com RD Station", leadId);
            throw;
        }
    }
}
