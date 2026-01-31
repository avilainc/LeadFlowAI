using MediatR;
using LeadFlowAI.Application.DTOs;

namespace LeadFlowAI.Application.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
}

public class RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}