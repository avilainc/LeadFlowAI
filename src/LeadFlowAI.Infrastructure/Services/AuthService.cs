using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LeadFlowAI.Application.DTOs;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;

namespace LeadFlowAI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> HashPasswordAsync(string password)
    {
        return await Task.Run(() =>
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        });
    }

    public async Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        var hashedPassword = await HashPasswordAsync(password);
        return hashedPassword == passwordHash;
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        return await Task.Run(() =>
        {
            var jwtKey = _configuration["JWT:Secret"] ?? _configuration["JWT:Key"] ?? _configuration["JWT_KEY"] ?? "default-jwt-key-for-development-must-be-at-least-32-characters-long";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("tenantId", user.TenantId.ToString()),
                new Claim("role", user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"] ?? "leadflowai",
                audience: _configuration["JWT:Audience"] ?? "leadflowai-users",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        });
    }

    public async Task<string> GenerateRefreshTokenAsync()
    {
        return await Task.Run(() =>
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        });
    }

    public async Task<UserDto> MapToUserDtoAsync(User user)
    {
        return await Task.Run(() => new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name ?? string.Empty
        });
    }
}