using FluentAssertions;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Domain.Tests;

public class LeadTests
{
    [Fact]
    public void Lead_Should_Allow_Setting_Status()
    {
        // Arrange
        var lead = new Lead();

        // Act
        lead.Status = LeadStatus.Received;

        // Assert
        lead.Status.Should().Be(LeadStatus.Received);
    }

    [Fact]
    public void Lead_Should_Initialize_With_Empty_Collections()
    {
        // Arrange & Act
        var lead = new Lead();

        // Assert
        lead.Events.Should().NotBeNull();
        lead.Events.Should().BeEmpty();
    }

    [Fact]
    public void Lead_Should_Allow_Setting_Basic_Properties()
    {
        // Arrange
        var lead = new Lead();
        var name = "João Silva";
        var email = "joao@email.com";
        var phone = "+5511999999999";
        var message = "Interessado em consultoria";

        // Act
        lead.Name = name;
        lead.Email = email;
        lead.Phone = phone;
        lead.Message = message;
        lead.Status = LeadStatus.Qualified;

        // Assert
        lead.Name.Should().Be(name);
        lead.Email.Should().Be(email);
        lead.Phone.Should().Be(phone);
        lead.Message.Should().Be(message);
        lead.Status.Should().Be(LeadStatus.Qualified);
    }

    [Fact]
    public void Lead_Should_Track_Response_Information()
    {
        // Arrange
        var lead = new Lead();
        var responseTime = DateTime.UtcNow;
        var channel = "WhatsApp";

        // Act
        lead.HasResponded = true;
        lead.RespondedAt = responseTime;
        lead.ResponseChannel = channel;

        // Assert
        lead.HasResponded.Should().BeTrue();
        lead.RespondedAt.Should().Be(responseTime);
        lead.ResponseChannel.Should().Be(channel);
    }

    [Fact]
    public void Lead_Should_Track_Handoff_Information()
    {
        // Arrange
        var lead = new Lead();
        var handoffTime = DateTime.UtcNow;
        var handedBy = "SDR João";

        // Act
        lead.IsHandedOff = true;
        lead.HandedOffAt = handoffTime;
        lead.HandedOffBy = handedBy;

        // Assert
        lead.IsHandedOff.Should().BeTrue();
        lead.HandedOffAt.Should().Be(handoffTime);
        lead.HandedOffBy.Should().Be(handedBy);
    }

    [Fact]
    public void Lead_Should_Store_LLM_Qualification_Data()
    {
        // Arrange
        var lead = new Lead();
        var score = 85;
        var intent = Intent.Orcamento;
        var urgency = Urgency.Alta;
        var services = new List<string> { "Consultoria", "Desenvolvimento" };
        var nextStep = RecommendedNextStep.Responder;

        // Act
        lead.LeadScore = score;
        lead.Intent = intent;
        lead.Urgency = urgency;
        lead.ServiceMatch = services;
        lead.RecommendedNextStep = nextStep;

        // Assert
        lead.LeadScore.Should().Be(score);
        lead.Intent.Should().Be(intent);
        lead.Urgency.Should().Be(urgency);
        lead.ServiceMatch.Should().BeEquivalentTo(services);
        lead.RecommendedNextStep.Should().Be(nextStep);
    }

    [Fact]
    public void Lead_Should_Track_Errors_And_Retries()
    {
        // Arrange
        var lead = new Lead();
        var errorMessage = "Falha na integração com WhatsApp";
        var retryCount = 3;

        // Act
        lead.LastError = errorMessage;
        lead.RetryCount = retryCount;

        // Assert
        lead.LastError.Should().Be(errorMessage);
        lead.RetryCount.Should().Be(retryCount);
    }
}
