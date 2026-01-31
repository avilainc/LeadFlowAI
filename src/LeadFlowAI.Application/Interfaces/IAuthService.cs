using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Domain.Entities;

namespace LeadFlowAI.Application.Interfaces;

public interface IAuthService
{
    Task<string> HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password, string passwordHash);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    Task<UserDto> MapToUserDtoAsync(User user);
}