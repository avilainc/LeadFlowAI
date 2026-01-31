# Blueprint - Testes Unitários e Integração do LeadFlowAI

## Visão Geral

Este blueprint define a estratégia de testes para o LeadFlowAI, focando em testes unitários e integração para garantir qualidade, manutenibilidade e confiabilidade do sistema. Seguindo os princípios de Clean Architecture, os testes serão organizados por camadas e responsabilidades.

## 1. Estrutura de Testes

### 1.1 Projetos de Teste
- **LeadFlowAI.Domain.Tests**: Testes para entidades, value objects e regras de negócio
- **LeadFlowAI.Application.Tests**: Testes para handlers MediatR, comandos, queries e validações
- **LeadFlowAI.Infrastructure.Tests**: Testes para repositórios, serviços externos e integrações
- **LeadFlowAI.WebAPI.Tests**: Testes de integração para endpoints da API
- **LeadFlowAI.Worker.Tests**: Testes para background jobs e processamento assíncrono

### 1.2 Tecnologias
- **xUnit**: Framework de testes
- **Moq**: Para mocks de dependências
- **FluentAssertions**: Para asserções mais legíveis
- **TestContainers**: Para testes de integração com bancos reais
- **Bogus**: Para geração de dados de teste (faker)

## 2. Estratégia de Testes por Camada

### 2.1 Domain Layer (Testes Unitários)

#### Entidades e Value Objects
```csharp
// LeadTests.cs
[Fact]
public void Lead_Should_Be_Created_With_Valid_Data()
{
    // Arrange
    var name = "João Silva";
    var email = "joao@email.com";
    var phone = "+5511999999999";

    // Act
    var lead = new Lead(name, email, phone);

    // Assert
    lead.Name.Should().Be(name);
    lead.Email.Should().Be(email);
    lead.Status.Should().Be(LeadStatus.New);
}
```

#### Enums e Regras de Negócio
```csharp
// LeadStatusTests.cs
[Theory]
[InlineData(LeadStatus.New, LeadStatus.Contacted)]
[InlineData(LeadStatus.Contacted, LeadStatus.Qualified)]
public void LeadStatus_Should_Allow_Valid_Transitions(LeadStatus from, LeadStatus to)
{
    // Act & Assert
    from.CanTransitionTo(to).Should().BeTrue();
}
```

### 2.2 Application Layer (Testes Unitários)

#### Handlers MediatR
```csharp
// IngestWebFormLeadHandlerTests.cs
[Fact]
public async Task Handle_Should_Ingest_Lead_And_Enqueue_Qualification()
{
    // Arrange
    var command = new IngestWebFormLeadCommand
    {
        Name = "Maria Santos",
        Email = "maria@email.com",
        Phone = "+5511988888888"
    };

    var mockRepo = new Mock<ILeadRepository>();
    var mockBus = new Mock<IMediator>();
    var handler = new IngestWebFormLeadHandler(mockRepo.Object, mockBus.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().Be(Unit.Value);
    mockRepo.Verify(x => x.AddAsync(It.IsAny<Lead>()), Times.Once);
    mockBus.Verify(x => x.Send(It.IsAny<QualifyLeadCommand>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

#### Validações e Commands
```csharp
// IngestWebFormLeadCommandValidatorTests.cs
[Fact]
public void Validate_Should_Fail_When_Email_Is_Invalid()
{
    // Arrange
    var validator = new IngestWebFormLeadCommandValidator();
    var command = new IngestWebFormLeadCommand
    {
        Name = "João",
        Email = "invalid-email",
        Phone = "+5511999999999"
    };

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(x => x.PropertyName == "Email");
}
```

### 2.3 Infrastructure Layer (Testes Unitários + Integração)

#### Repositórios
```csharp
// LeadRepositoryTests.cs
[Fact]
public async Task GetByIdAsync_Should_Return_Lead_When_Exists()
{
    // Arrange
    var leadId = Guid.NewGuid();
    var expectedLead = new Lead("Test", "test@email.com", "+5511999999999") { Id = leadId };

    var mockContext = new Mock<IMongoContext>();
    mockContext.Setup(x => x.Leads.Find(It.IsAny<FilterDefinition<Lead>>()))
        .Returns(new Mock<IAsyncCursor<Lead>>().Object);

    var repo = new LeadRepository(mockContext.Object);

    // Act
    var result = await repo.GetByIdAsync(leadId);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(leadId);
}
```

#### Serviços Externos (com Mocks)
```csharp
// WhatsAppServiceTests.cs
[Fact]
public async Task SendMessageAsync_Should_Call_Twilio_Api()
{
    // Arrange
    var mockTwilio = new Mock<ITwilioRestClient>();
    var service = new WhatsAppService(mockTwilio.Object);

    // Act
    await service.SendMessageAsync("+5511999999999", "Olá!");

    // Assert
    mockTwilio.Verify(x => x.RequestAsync(It.IsAny<Request>()), Times.Once);
}
```

### 2.4 WebAPI Layer (Testes de Integração)

#### Testes de Endpoint
```csharp
// LeadsControllerTests.cs
public class LeadsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LeadsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLeads_Should_Return_200_With_Leads()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/leads");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("leads");
    }
}
```

#### Testes com Banco Real (TestContainers)
```csharp
// IntegrationTests.cs
[Fact]
public async Task Full_Ingest_And_Qualify_Flow_Should_Work()
{
    // Arrange - Setup MongoDB container
    var mongoContainer = new MongoDbContainer("mongo:6.0");
    await mongoContainer.StartAsync();

    // Configure services with real MongoDB
    var services = new ServiceCollection();
    services.AddSingleton<IMongoClient>(new MongoClient(mongoContainer.GetConnectionString()));

    // Act - Execute full flow
    // Assert - Verify results in database
}
```

### 2.5 Worker Layer (Testes de Background Jobs)

#### BackgroundJobProcessor
```csharp
// BackgroundJobProcessorTests.cs
[Fact]
public async Task ProcessQualificationJob_Should_Update_Lead_Status()
{
    // Arrange
    var lead = new Lead("Test", "test@email.com", "+5511999999999");
    var mockRepo = new Mock<ILeadRepository>();
    var mockLLM = new Mock<ILLMService>();
    var processor = new BackgroundJobProcessor(mockRepo.Object, mockLLM.Object);

    mockLLM.Setup(x => x.QualifyLeadAsync(It.IsAny<Lead>()))
        .ReturnsAsync(new LLMQualificationResult { IsQualified = true, Score = 85 });

    // Act
    await processor.ProcessQualificationJob(lead.Id);

    // Assert
    mockRepo.Verify(x => x.UpdateAsync(It.Is<Lead>(l => l.Status == LeadStatus.Qualified)), Times.Once);
}
```

## 3. Cobertura e Métricas

### 3.1 Cobertura Mínima
- **Domain**: 95% (entidades críticas)
- **Application**: 90% (handlers e validações)
- **Infrastructure**: 80% (repositórios e serviços)
- **WebAPI**: 85% (endpoints principais)
- **Worker**: 85% (jobs críticos)

### 3.2 Tipos de Testes
- **Unitários**: 70% do total
- **Integração**: 25%
- **E2E**: 5% (futuro)

## 4. Configuração CI/CD

### 4.1 GitHub Actions
```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      mongodb:
        image: mongo:6.0
        ports:
          - 27017:27017
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

### 4.2 Configuração de Testes
- **TestSettings.json**: Configurações específicas para testes
- **TestData**: Dados de teste estáticos e gerados
- **TestContainers**: Para dependências externas (MongoDB, Redis)

## 5. Boas Práticas

### 5.1 Padrões de Nomenclatura
- `ClasseTeste.cs` para classes de teste
- `Metodo_Scenario_Resultado` para nomes de métodos
- `Arrange_Act_Assert` para estrutura dos testes

### 5.2 Mocks vs Fakes
- **Mocks**: Para dependências externas (APIs, bancos)
- **Fakes**: Para objetos complexos internos
- **Stubs**: Para dados simples

### 5.3 Test Data Management
- **Builders**: Para construção de objetos complexos
- **Factories**: Para criação de dados de teste
- **Bogus**: Para geração aleatória mas consistente

## 6. Roadmap de Implementação

### Fase 1: Foundation
- [ ] Criar projetos de teste
- [ ] Configurar xUnit, Moq, FluentAssertions
- [ ] Testes básicos de entidades

### Fase 2: Core Logic
- [ ] Testes de handlers MediatR
- [ ] Validações de comandos/queries
- [ ] Regras de negócio

### Fase 3: Infrastructure
- [ ] Testes de repositórios
- [ ] Mocks de serviços externos
- [ ] Testes de integração com MongoDB

### Fase 4: Integration & CI
- [ ] Testes de API endpoints
- [ ] Testes de background jobs
- [ ] Configuração CI/CD
- [ ] Relatórios de cobertura

Este blueprint garante que o LeadFlowAI tenha uma base sólida de testes, facilitando refatorações, novas funcionalidades e deploy confiável.