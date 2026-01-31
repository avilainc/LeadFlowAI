using FluentAssertions;
using LeadFlowAI.Domain.Enums;

namespace LeadFlowAI.Domain.Tests;

public class LeadStatusTests
{
    [Theory]
    [InlineData(LeadStatus.Received, LeadStatus.Normalized)]
    [InlineData(LeadStatus.Normalized, LeadStatus.Enriched)]
    [InlineData(LeadStatus.Enriched, LeadStatus.Qualified)]
    [InlineData(LeadStatus.Qualified, LeadStatus.Responded)]
    [InlineData(LeadStatus.Responded, LeadStatus.Handoff)]
    [InlineData(LeadStatus.Handoff, LeadStatus.Closed)]
    public void LeadStatus_Should_Have_Logical_Order(LeadStatus current, LeadStatus next)
    {
        // Assert
        ((int)next).Should().BeGreaterThan((int)current);
    }

    [Fact]
    public void LeadStatus_Should_Have_All_Required_Values()
    {
        // Arrange
        var expectedStatuses = new[]
        {
            LeadStatus.Received,
            LeadStatus.Normalized,
            LeadStatus.Enriched,
            LeadStatus.Qualified,
            LeadStatus.Responded,
            LeadStatus.Handoff,
            LeadStatus.Closed,
            LeadStatus.Failed
        };

        // Act
        var actualStatuses = Enum.GetValues<LeadStatus>();

        // Assert
        actualStatuses.Should().BeEquivalentTo(expectedStatuses);
    }

    [Fact]
    public void LeadStatus_Failed_Should_Be_Highest_Value()
    {
        // Arrange
        var allStatuses = Enum.GetValues<LeadStatus>();
        var maxValue = allStatuses.Max(s => (int)s);

        // Assert
        ((int)LeadStatus.Failed).Should().Be(maxValue);
    }
}