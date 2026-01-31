using FluentAssertions;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Validators;

namespace LeadFlowAI.Application.Tests;

public class AuthValidatorsTests
{
    [Fact]
    public void LoginCommandValidator_Should_Pass_When_Data_Is_Valid()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void LoginCommandValidator_Should_Fail_When_Email_Is_Empty(string? email)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Email = email!,
            Password = "password123"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("obrigatório"));
    }

    [Fact]
    public void LoginCommandValidator_Should_Fail_When_Email_Is_Invalid()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "password123"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("válido"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void LoginCommandValidator_Should_Fail_When_Password_Is_Empty(string? password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = password!
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("obrigatória"));
    }

    [Fact]
    public void RegisterCommandValidator_Should_Pass_When_Data_Is_Valid()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void RegisterCommandValidator_Should_Fail_When_Password_Is_Too_Short()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "12345", // Too short
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("8 caracteres"));
    }

    [Fact]
    public void RegisterCommandValidator_Should_Fail_When_Password_Has_No_Uppercase()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "password123", // No uppercase
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("maiúscula"));
    }

    [Fact]
    public void RegisterCommandValidator_Should_Fail_When_Password_Has_No_Number()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password", // No number
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("número"));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void RegisterCommandValidator_Should_Fail_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = email,
            Password = "Password123",
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RegisterCommandValidator_Should_Fail_When_FirstName_Is_Empty(string? firstName)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = firstName!,
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void RegisterCommandValidator_Should_Fail_When_FirstName_Is_Too_Long()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = new string('A', 51), // 51 characters
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = "minha-empresa"
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("máximo 50"));
    }

    [Theory]
    [InlineData("test slug")]
    [InlineData("Test@Slug")]
    [InlineData("test_slug!")]
    public void RegisterCommandValidator_Should_Fail_When_TenantSlug_Is_Invalid(string tenantSlug)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = tenantSlug
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantSlug");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("letras minúsculas, números e hífens"));
    }

    [Fact]
    public void RegisterCommandValidator_Should_Fail_When_TenantSlug_Is_Too_Long()
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "João",
            LastName = "Silva",
            TenantName = "Minha Empresa",
            TenantSlug = new string('a', 51) // 51 characters
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantSlug");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("máximo 50"));
    }
}