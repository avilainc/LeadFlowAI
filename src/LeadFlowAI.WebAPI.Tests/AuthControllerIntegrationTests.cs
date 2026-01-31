using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeadFlowAI.WebAPI.Tests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_Return_Created_When_Data_Is_Valid()
    {
        // Arrange
        var registerRequest = new RegisterCommand
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            TenantName = "Test Company",
            TenantSlug = $"test-company-{Guid.NewGuid()}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        // Arrange
        var email = $"existing{Guid.NewGuid()}@example.com";
        var firstRegisterRequest = new RegisterCommand
        {
            Email = email,
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            TenantName = "Test Company",
            TenantSlug = $"test-company-{Guid.NewGuid()}"
        };

        // Register first user
        await _client.PostAsJsonAsync("/api/auth/register", firstRegisterRequest);

        // Try to register with same email
        var secondRegisterRequest = new RegisterCommand
        {
            Email = email,
            Password = "password456",
            FirstName = "Test2",
            LastName = "User2",
            TenantName = "Test Company 2",
            TenantSlug = $"test-company-2-{Guid.NewGuid()}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", secondRegisterRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_Ok_With_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var email = $"login{Guid.NewGuid()}@example.com";
        var password = "password123";

        // First register a user
        var registerRequest = new RegisterCommand
        {
            Email = email,
            Password = password,
            FirstName = "Login",
            LastName = "Test",
            TenantName = "Login Test Company",
            TenantSlug = $"login-company-{Guid.NewGuid()}"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Now try to login
        var loginRequest = new LoginCommand
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_When_Credentials_Are_Invalid()
    {
        // Arrange
        var loginRequest = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_New_Token_When_Valid_Refresh_Token_Provided()
    {
        // Arrange
        var email = $"refresh{Guid.NewGuid()}@example.com";

        // First register and login to get tokens
        var registerRequest = new RegisterCommand
        {
            Email = email,
            Password = "password123",
            FirstName = "Refresh",
            LastName = "Test",
            TenantName = "Refresh Test Company",
            TenantSlug = $"refresh-company-{Guid.NewGuid()}"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Now refresh the token
        var refreshRequest = new RefreshTokenCommand
        {
            RefreshToken = loginResult!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);

        // Tokens should be different from the original ones
        result.AccessToken.Should().NotBe(loginResult.AccessToken);
        result.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }
}