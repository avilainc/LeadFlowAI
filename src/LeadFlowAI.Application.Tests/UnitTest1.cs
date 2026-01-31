using FluentAssertions;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Handlers;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Domain.Enums;
using Moq;

namespace LeadFlowAI.Application.Tests;

public class AuthHandlersTests
{
    [Fact]
    public async Task LoginHandler_Should_Return_LoginResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            IsActive = true
        };

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);
        mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(x => x.VerifyPasswordAsync(command.Password, user.PasswordHash))
            .ReturnsAsync(true);
        mockAuthService.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync("jwt-token");
        mockAuthService.Setup(x => x.GenerateRefreshTokenAsync())
            .ReturnsAsync("refresh-token");
        mockAuthService.Setup(x => x.MapToUserDtoAsync(user))
            .ReturnsAsync(new Application.DTOs.UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                TenantId = user.TenantId
            });

        var handler = new LoginHandler(mockUserRepo.Object, mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(user.Email);

        mockUserRepo.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
        mockAuthService.Verify(x => x.VerifyPasswordAsync(command.Password, user.PasswordHash), Times.Once);
        mockAuthService.Verify(x => x.GenerateJwtTokenAsync(user), Times.Once);
        mockAuthService.Verify(x => x.GenerateRefreshTokenAsync(), Times.Once);
        mockUserRepo.Verify(x => x.UpdateAsync(It.Is<User>(u => u.LastLoginAt.HasValue)), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginHandler_Should_Throw_UnauthorizedAccessException_When_User_Not_Found()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var mockAuthService = new Mock<IAuthService>();
        var handler = new LoginHandler(mockUserRepo.Object, mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, default));

        mockUserRepo.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
        mockAuthService.Verify(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginHandler_Should_Throw_UnauthorizedAccessException_When_Password_Is_Invalid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(x => x.VerifyPasswordAsync(command.Password, user.PasswordHash))
            .ReturnsAsync(false);

        var handler = new LoginHandler(mockUserRepo.Object, mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, default));

        mockAuthService.Verify(x => x.VerifyPasswordAsync(command.Password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task RegisterHandler_Should_Create_Tenant_And_User_When_Data_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "newuser@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User",
            TenantName = "Test Company",
            TenantSlug = "test-company"
        };

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);
        mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = userId)
            .Returns(Task.CompletedTask);
        mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var mockTenantRepo = new Mock<ITenantRepository>();
        mockTenantRepo.Setup(x => x.GetBySlugAsync(command.TenantSlug, default))
            .ReturnsAsync((Tenant?)null);
        mockTenantRepo.Setup(x => x.AddAsync(It.IsAny<Tenant>(), default))
            .Callback<Tenant, CancellationToken>((t, ct) => t.Id = tenantId)
            .Returns(Task.CompletedTask);

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(x => x.HashPasswordAsync(command.Password))
            .ReturnsAsync("hashedpassword");
        mockAuthService.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("jwt-token");
        mockAuthService.Setup(x => x.GenerateRefreshTokenAsync())
            .ReturnsAsync("refresh-token");
        mockAuthService.Setup(x => x.MapToUserDtoAsync(It.IsAny<User>()))
            .ReturnsAsync(new Application.DTOs.UserDto
            {
                Id = userId,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Role = UserRole.Owner.ToString(),
                TenantId = tenantId
            });

        var handler = new RegisterHandler(mockUserRepo.Object, mockTenantRepo.Object, mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(command.Email);
        result.User.Role.Should().Be(UserRole.Owner.ToString());

        mockTenantRepo.Verify(x => x.AddAsync(It.Is<Tenant>(t =>
            t.Name == command.TenantName &&
            t.Slug == command.TenantSlug), default), Times.Once);

        mockUserRepo.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == command.Email &&
            u.FirstName == command.FirstName &&
            u.LastName == command.LastName &&
            u.Role == UserRole.Owner)), Times.Once);
    }

    [Fact]
    public async Task RegisterHandler_Should_Throw_InvalidOperationException_When_Email_Already_Exists()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "password123",
            FirstName = "Existing",
            LastName = "User",
            TenantName = "Test Company",
            TenantSlug = "test-company"
        };

        var existingUser = new User { Email = command.Email };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        var mockTenantRepo = new Mock<ITenantRepository>();
        var mockAuthService = new Mock<IAuthService>();

        var handler = new RegisterHandler(mockUserRepo.Object, mockTenantRepo.Object, mockAuthService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, default));

        exception.Message.Should().Be("Email já está em uso");

        mockTenantRepo.Verify(x => x.AddAsync(It.IsAny<Tenant>(), default), Times.Never);
        mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterHandler_Should_Throw_InvalidOperationException_When_Tenant_Slug_Already_Exists()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "newuser@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User",
            TenantName = "Test Company",
            TenantSlug = "existing-slug"
        };

        var existingTenant = new Tenant { Slug = command.TenantSlug };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var mockTenantRepo = new Mock<ITenantRepository>();
        mockTenantRepo.Setup(x => x.GetBySlugAsync(command.TenantSlug, default))
            .ReturnsAsync(existingTenant);

        var mockAuthService = new Mock<IAuthService>();

        var handler = new RegisterHandler(mockUserRepo.Object, mockTenantRepo.Object, mockAuthService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, default));

        exception.Message.Should().Be("Slug da empresa já está em uso");

        mockTenantRepo.Verify(x => x.AddAsync(It.IsAny<Tenant>(), default), Times.Never);
        mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
