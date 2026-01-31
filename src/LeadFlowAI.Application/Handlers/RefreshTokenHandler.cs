using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Interfaces;

namespace LeadFlowAI.Application.Handlers;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public RefreshTokenHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null || !user.IsActive || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");
        }

        // Generate new tokens
        var accessToken = await _authService.GenerateJwtTokenAsync(user);
        var refreshToken = await _authService.GenerateRefreshTokenAsync();

        // Update refresh token
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