using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using LeadFlowAI.Application.Queries;
using LeadFlowAI.Application.DTOs;
using System.Security.Claims;

namespace LeadFlowAI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(IMediator mediator, ILogger<LeadsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Buscar lead por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantIdFromToken();
        
        var query = new GetLeadByIdQuery { LeadId = id, TenantId = tenantId };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Buscar eventos/timeline de um lead
    /// </summary>
    [HttpGet("{id}/events")]
    [ProducesResponseType(typeof(List<LeadEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(Guid id)
    {
        var query = new GetLeadEventsQuery { LeadId = id };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Pesquisar leads com filtros
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchLeadsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? status,
        [FromQuery] string? source,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantIdFromToken();

        var searchQuery = new SearchLeadsQuery
        {
            TenantId = tenantId,
            Query = query,
            Status = status,
            Source = source,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var (leads, total) = await _mediator.Send(searchQuery);

        return Ok(new SearchLeadsResponse
        {
            Leads = leads,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    /// <summary>
    /// Assumir conversa (handoff para humano)
    /// </summary>
    [HttpPost("{id}/handoff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handoff(Guid id)
    {
        // Implementar comando de handoff
        _logger.LogInformation("Lead {LeadId} assumido por usuário", id);
        return Ok(new { message = "Lead assumido com sucesso" });
    }

    private Guid GetTenantIdFromToken()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class SearchLeadsResponse
{
    public List<LeadDto> Leads { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
