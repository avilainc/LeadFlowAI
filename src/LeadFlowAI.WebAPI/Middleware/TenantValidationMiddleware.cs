using LeadFlowAI.Domain.Interfaces;

namespace LeadFlowAI.WebAPI.Middleware;

public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantValidationMiddleware> _logger;

    public TenantValidationMiddleware(RequestDelegate next, ILogger<TenantValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository)
    {
        // Skip middleware for authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Extract tenant ID from token
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            _logger.LogWarning("Invalid or missing tenant ID in token for user {UserId}", context.User.FindFirst("sub")?.Value);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant ID" });
            return;
        }

        // Validate tenant exists
        var tenant = await tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        // Add tenant to HttpContext for easy access
        context.Items["Tenant"] = tenant;

        await _next(context);
    }
}