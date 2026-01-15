using Microsoft.AspNetCore.Mvc;
using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.WebAPI.Controllers;

[ApiController]
[Route("api/leads/ingest")]
public class LeadIngestionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeadIngestionController> _logger;

    public LeadIngestionController(IMediator mediator, ILogger<LeadIngestionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para receber leads de formulários web
    /// </summary>
    [HttpPost("webform")]
    [ProducesResponseType(typeof(LeadIngestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestWebForm([FromBody] WebFormLeadRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo lead de webform: {Name} - {TenantSlug}", request.Name, request.TenantSlug);

            var command = new IngestWebFormLeadCommand
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company,
                City = request.City,
                State = request.State,
                Message = request.Message,
                UtmSource = request.UtmSource,
                UtmCampaign = request.UtmCampaign,
                UtmMedium = request.UtmMedium,
                UtmContent = request.UtmContent,
                Gclid = request.Gclid,
                Fbclid = request.Fbclid,
                SourceUrl = request.SourceUrl,
                TenantSlug = request.TenantSlug
            };

            var leadId = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetLead), "Leads", new { id = leadId }, new LeadIngestionResponse
            {
                LeadId = leadId,
                Message = "Lead recebido com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar lead de webform");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Webhook para receber leads do RD Station
    /// </summary>
    [HttpPost("rdstation/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RDStationWebhook([FromBody] RDStationWebhookRequest request, [FromQuery] string tenantSlug)
    {
        try
        {
            _logger.LogInformation("Recebendo webhook do RD Station: {EventType} - {TenantSlug}", request.EventType, tenantSlug);

            // Verificar assinatura do webhook (implementar validação)
            // var signature = Request.Headers["X-RD-Signature"];

            var command = new IngestRDStationLeadCommand
            {
                EventType = request.EventType,
                Payload = request.Payload,
                TenantSlug = tenantSlug
            };

            var leadId = await _mediator.Send(command);

            return Ok(new { leadId, message = "Webhook processado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do RD Station");
            return BadRequest(new { error = ex.Message });
        }
    }

    // Método auxiliar para redirecionamento
    [NonAction]
    public IActionResult GetLead(Guid id)
    {
        return Ok();
    }
}

public class LeadIngestionResponse
{
    public Guid LeadId { get; set; }
    public string Message { get; set; } = string.Empty;
}
