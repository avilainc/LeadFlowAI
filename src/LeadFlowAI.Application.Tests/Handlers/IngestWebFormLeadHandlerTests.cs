using FluentAssertions;
using LeadFlowAI.Application.Commands;
using LeadFlowAI.Application.Handlers;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Domain.Enums;
using Moq;
using Xunit;

namespace LeadFlowAI.Application.Tests.Handlers;

public class IngestWebFormLeadHandlerTests
{
    [Fact]
    public async Task Handle_Should_Ingest_Lead_Successfully()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = "test-tenant",
            Name = "Test Tenant",
            IsActive = true
        };

        var command = new IngestWebFormLeadCommand
        {
            TenantSlug = "test-tenant",
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "+5511999999999",
            Message = "Interessado no produto",
            UtmSource = "google",
            UtmCampaign = "summer-sale"
        };

        var mockTenantRepo = new Mock<ITenantRepository>();
        mockTenantRepo.Setup(x => x.GetBySlugAsync(command.TenantSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var mockLeadRepo = new Mock<ILeadRepository>();
        mockLeadRepo.Setup(x => x.GetByDeduplicationHashAsync(It.IsAny<string>(), tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        var mockEventRepo = new Mock<ILeadEventRepository>();
        var mockIdempotency = new Mock<IIdempotencyService>();

        var mockBackgroundJob = new Mock<IBackgroundJobService>();
        var mockPhoneNormalizer = new Mock<IPhoneNormalizer>();
        mockPhoneNormalizer.Setup(x => x.NormalizeToE164(command.Phone, "BR"))
            .Returns("+5511999999999");

        var handler = new IngestWebFormLeadHandler(
            mockTenantRepo.Object,
            mockLeadRepo.Object,
            mockEventRepo.Object,
            mockIdempotency.Object,
            mockBackgroundJob.Object,
            mockPhoneNormalizer.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeEmpty();
        mockTenantRepo.Verify(x => x.GetBySlugAsync(command.TenantSlug, It.IsAny<CancellationToken>()), Times.Once);
        mockLeadRepo.Verify(x => x.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventRepo.Verify(x => x.AddAsync(It.IsAny<LeadEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Tenant_Not_Found()
    {
        // Arrange
        var command = new IngestWebFormLeadCommand
        {
            TenantSlug = "nonexistent-tenant",
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "+5511999999999"
        };

        var mockTenantRepo = new Mock<ITenantRepository>();
        mockTenantRepo.Setup(x => x.GetBySlugAsync(command.TenantSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var mockLeadRepo = new Mock<ILeadRepository>();
        var mockEventRepo = new Mock<ILeadEventRepository>();
        var mockIdempotency = new Mock<IIdempotencyService>();
        var mockBackgroundJob = new Mock<IBackgroundJobService>();
        var mockPhoneNormalizer = new Mock<IPhoneNormalizer>();

        var handler = new IngestWebFormLeadHandler(
            mockTenantRepo.Object,
            mockLeadRepo.Object,
            mockEventRepo.Object,
            mockIdempotency.Object,
            mockBackgroundJob.Object,
            mockPhoneNormalizer.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, default));
        mockTenantRepo.Verify(x => x.GetBySlugAsync(command.TenantSlug, It.IsAny<CancellationToken>()), Times.Once);
    }
}