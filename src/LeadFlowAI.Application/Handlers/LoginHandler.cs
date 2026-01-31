using MediatR;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;

namespace LeadFlowAI.Application.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        var isPasswordValid = await _authService.VerifyPasswordAsync(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

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