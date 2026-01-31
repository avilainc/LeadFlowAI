using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Application.Handlers;

public class RegisterHandler : IRequestHandler<RegisterCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IAuthService _authService;

    public RegisterHandler(IUserRepository userRepository, ITenantRepository tenantRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email já está em uso");
        }

        // Check if tenant slug already exists
        var existingTenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug);
        if (existingTenant != null)
        {
            throw new InvalidOperationException("Slug da empresa já está em uso");
        }

        // Create tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            Slug = request.TenantSlug,
            Domain = $"{request.TenantSlug}.leadflow.ai",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant);

        // Create user
        var passwordHash = await _authService.HashPasswordAsync(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.Owner,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Generate tokens
        var accessToken = await _authService.GenerateJwtTokenAsync(user);
        var refreshToken = await _authService.GenerateRefreshTokenAsync();

        // Store refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        var userDto = await _authService.MapToUserDtoAsync(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = userDto
        };
    }
}