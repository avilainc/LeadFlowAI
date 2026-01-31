using FluentAssertions;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;
using LeadFlowAI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LeadFlowAI.Infrastructure.Tests;

public class InfrastructureServicesTests
{
    [Fact]
    public async Task AuthService_Should_Hash_Password()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var authService = new AuthService(config.Object);

        var password = "testpassword123";

        // Act
        var hash = await authService.HashPasswordAsync(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public async Task AuthService_Should_Verify_Correct_Password()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var authService = new AuthService(config.Object);

        var password = "testpassword123";
        var hash = await authService.HashPasswordAsync(password);

        // Act
        var isValid = await authService.VerifyPasswordAsync(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task AuthService_Should_Reject_Incorrect_Password()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var authService = new AuthService(config.Object);

        var password = "testpassword123";
        var wrongPassword = "wrongpassword";
        var hash = await authService.HashPasswordAsync(password);

        // Act
        var isValid = await authService.VerifyPasswordAsync(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task AuthService_Should_Generate_Jwt_Token()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        config.Setup(x => x["JWT:Key"]).Returns("supersecretkeythatislongenoughforjwttokensandhasatleast32bytes");
        config.Setup(x => x["Jwt:Issuer"]).Returns("testissuer");
        config.Setup(x => x["Jwt:Audience"]).Returns("testaudience");

        var authService = new AuthService(config.Object);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid()
        };

        // Act
        var token = await authService.GenerateJwtTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthService_Should_Generate_Refresh_Token()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var authService = new AuthService(config.Object);

        // Act
        var refreshToken = await authService.GenerateRefreshTokenAsync();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(20); // Should be a decent length
    }

    [Fact]
    public async Task AuthService_Should_Map_User_To_Dto()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var authService = new AuthService(config.Object);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid()
        };

        // Act
        var userDto = await authService.MapToUserDtoAsync(user);

        // Assert
        userDto.Should().NotBeNull();
        userDto.Id.Should().Be(user.Id);
        userDto.Email.Should().Be(user.Email);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.Role.Should().Be(user.Role.ToString());
        userDto.TenantId.Should().Be(user.TenantId);
    }
}