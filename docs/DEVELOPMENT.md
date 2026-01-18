# Guia de Desenvolvimento - LeadFlowAI

## Setup do Ambiente de Desenvolvimento

### 1. Instalar Ferramentas

#### Windows
```powershell
# Instalar .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Instalar Node.js
winget install OpenJS.NodeJS

# Instalar Docker Desktop
winget install Docker.DockerDesktop

# Instalar Visual Studio Code
winget install Microsoft.VisualStudioCode
```

#### macOS
```bash
# Usando Homebrew
brew install --cask dotnet-sdk
brew install node
brew install --cask docker
brew install --cask visual-studio-code
```

#### Linux (Ubuntu/Debian)
```bash
# .NET 8
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

# Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# Docker
sudo apt-get install docker.io docker-compose
```

### 2. Extensões Recomendadas (VS Code)

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "ms-azuretools.vscode-docker",
    "dbaeumer.vscode-eslint",
    "esbenp.prettier-vscode",
    "bradlc.vscode-tailwindcss",
    "ms-vscode.vscode-typescript-next"
  ]
}
```

### 3. Configurar Banco de Dados Local

```bash
# Iniciar PostgreSQL via Docker
docker run --name leadflowai-postgres \
  -e POSTGRES_USER=leadflowai \
  -e POSTGRES_PASSWORD=leadflowai_password \
  -e POSTGRES_DB=leadflowai \
  -p 5432:5432 \
  -d postgres:15-alpine

# Executar script de inicialização
docker exec -i leadflowai-postgres psql -U leadflowai -d leadflowai < database/init.sql
```

### 4. Configurar Variáveis de Ambiente

```bash
# Copiar e editar
cp .env.example .env

# Mínimo necessário para rodar local:
JWT_SECRET=dev-secret-key-change-in-production-minimum-32-chars
OPENAI_API_KEY=sk-your-api-key-here
```

## Estrutura de Branches

```
main (produção)
├── develop (integração)
│   ├── feature/nome-da-feature
│   ├── bugfix/nome-do-bug
│   └── hotfix/nome-do-hotfix
```

### Convenção de Commits

Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: adiciona endpoint de estatísticas
fix: corrige cálculo de score
docs: atualiza README com instruções de deploy
refactor: reorganiza estrutura de pastas
test: adiciona testes para LeadRepository
chore: atualiza dependências
```

## Executando Localmente

### Backend

```bash
# Terminal 1: API
cd src/LeadFlowAI.WebAPI
dotnet watch run

# Terminal 2: Worker
cd src/LeadFlowAI.Worker
dotnet watch run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Acessar:
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Hangfire: http://localhost:5000/hangfire
- Frontend: http://localhost:3000

## Adicionando Nova Feature

### 1. Criar Nova Entidade (Domain)

```csharp
// src/LeadFlowAI.Domain/Entities/MinhaEntidade.cs
public class MinhaEntidade
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 2. Adicionar ao DbContext (Infrastructure)

```csharp
// src/LeadFlowAI.Infrastructure/Persistence/ApplicationDbContext.cs
public DbSet<MinhaEntidade> MinhasEntidades { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MinhaEntidade>(entity =>
    {
        entity.ToTable("minhas_entidades");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
    });
}
```

### 3. Criar Migration

```bash
cd src/LeadFlowAI.Infrastructure
dotnet ef migrations add AdicionaMinhaEntidade --startup-project ../LeadFlowAI.WebAPI
dotnet ef database update --startup-project ../LeadFlowAI.WebAPI
```

### 4. Criar Command/Query (Application)

```csharp
// src/LeadFlowAI.Application/Commands/CriarMinhaEntidadeCommand.cs
public class CriarMinhaEntidadeCommand : IRequest<Guid>
{
    public string Nome { get; set; } = string.Empty;
}

// src/LeadFlowAI.Application/Handlers/CriarMinhaEntidadeHandler.cs
public class CriarMinhaEntidadeHandler : IRequestHandler<CriarMinhaEntidadeCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    
    public CriarMinhaEntidadeHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Guid> Handle(CriarMinhaEntidadeCommand request, CancellationToken cancellationToken)
    {
        var entidade = new MinhaEntidade
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            CreatedAt = DateTime.UtcNow
        };
        
        await _context.MinhasEntidades.AddAsync(entidade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entidade.Id;
    }
}
```

### 5. Criar Endpoint (WebAPI)

```csharp
// src/LeadFlowAI.WebAPI/Controllers/MinhaEntidadeController.cs
[ApiController]
[Route("api/[controller]")]
public class MinhaEntidadeController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public MinhaEntidadeController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMinhaEntidadeCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        // Implementar query
        return Ok();
    }
}
```

## Testes

### Testes Unitários

```csharp
// tests/LeadFlowAI.Application.Tests/Handlers/IngestWebFormLeadHandlerTests.cs
public class IngestWebFormLeadHandlerTests
{
    [Fact]
    public async Task Handle_DevecriarLead_QuandoDadosValidos()
    {
        // Arrange
        var mockTenantRepo = new Mock<ITenantRepository>();
        var mockLeadRepo = new Mock<ILeadRepository>();
        // ... outros mocks
        
        var handler = new IngestWebFormLeadHandler(
            mockTenantRepo.Object,
            mockLeadRepo.Object,
            // ...
        );
        
        var command = new IngestWebFormLeadCommand
        {
            Name = "João Silva",
            Phone = "+5511999999999",
            // ...
        };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotEqual(Guid.Empty, result);
        mockLeadRepo.Verify(x => x.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

## Debug

### API

1. No VS Code, pressione F5
2. Ou adicione breakpoints e use "Run and Debug"

### Worker

```bash
# Executar com logs detalhados
cd src/LeadFlowAI.Worker
dotnet run --configuration Debug
```

### Frontend

```bash
# Modo dev com source maps
cd frontend
npm run dev
```

## Troubleshooting

### Problema: "Cannot connect to PostgreSQL"

```bash
# Verificar se container está rodando
docker ps | grep postgres

# Ver logs
docker logs leadflowai-postgres

# Recriar container
docker stop leadflowai-postgres
docker rm leadflowai-postgres
# Executar comando de criação novamente
```

### Problema: "OpenAI API rate limit"

```bash
# Adicionar delay entre chamadas ou usar modelo mais barato
LLM_MODEL=gpt-3.5-turbo
```

### Problema: "Hangfire not processing jobs"

```bash
# Verificar conexão com Redis
docker ps | grep redis

# Ver logs do worker
docker logs leadflowai-worker

# Acessar Hangfire dashboard e verificar "Failed Jobs"
```

## Boas Práticas

### C# / .NET

- ✅ Usar `async/await` para operações I/O
- ✅ Injeção de dependência via construtor
- ✅ Nomear interfaces com `I` prefixo
- ✅ DTOs imutáveis (init ou readonly)
- ✅ Validar entrada com FluentValidation
- ✅ Usar `CancellationToken` em métodos assíncronos

### React / TypeScript

- ✅ Componentes funcionais com hooks
- ✅ Tipos explícitos (evitar `any`)
- ✅ React Query para cache e sincronização de estado servidor
- ✅ Tailwind para estilização
- ✅ Extrair lógica complexa em hooks customizados

### Git

- ✅ Commits pequenos e atômicos
- ✅ Mensagens descritivas
- ✅ Pull request com descrição clara
- ✅ Code review antes de merge
- ✅ Rebase antes de merge na develop

## Performance

### Backend

- ✅ Usar `AsNoTracking()` em queries de leitura
- ✅ Índices nas colunas usadas em WHERE/JOIN
- ✅ Paginação obrigatória em listas
- ✅ Cache de configs de tenant no Redis
- ✅ Connection pooling no PostgreSQL

### Frontend

- ✅ Lazy loading de rotas
- ✅ Debounce em inputs de busca
- ✅ Virtual scrolling para listas grandes
- ✅ Otimização de imagens
- ✅ Code splitting

## Recursos

- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [EF Core Best Practices](https://learn.microsoft.com/ef/core/)
- [React Documentation](https://react.dev/)
- [TailwindCSS](https://tailwindcss.com/docs)
- [Hangfire Documentation](https://docs.hangfire.io/)
