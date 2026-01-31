# Avaliação da Arquitetura do LeadFlowAI

A seguir está uma avaliação detalhada da arquitetura do **LeadFlowAI** e um plano de **seis pull requests (PRs)** para levar o projeto até um deploy pronto para produção.

## Avaliação da arquitetura

O repositório segue uma **Clean Architecture** com projetos separados por responsabilidades:

* **Domain** define entidades como *Lead*, *Tenant* (com configurações como horário de funcionamento e FAQ) e value objects como `BusinessHours`. Há interfaces de repositório e serviços que facilitam testes e substituições.
* **Application** implementa **handlers** MediatR para ingestão de leads, qualificação com LLM e envio de respostas. O código normaliza telefones, enfileira tarefas e aplica guardrails, demonstrando um fluxo bem estruturado.
* **Infrastructure** fornece implementações concretas: EF Core para persistência, serviços de envio de e-mail (SendGrid), WhatsApp (Twilio), integração com RD Station e LLM (OpenAI).
* **Worker** usa Hangfire para processar tarefas em background; `BackgroundJobProcessor` executa qualificação, envio de mensagens e sincronização com RD Station.
* Existe documentação de arquitetura e deploy em `docs/ARCHITECTURE.md` e `docs/DEPLOY.md` descrevendo a proposta e listando próximos passos (como testes, CI/CD, autenticação).
* A aplicação conta com um **frontend** em React/TypeScript e workflow para GitHub Pages, mas o step de deploy falha se o Pages não estiver habilitado ou se não houver `package‑lock.json`.

Pontos positivos: modularidade, uso de padrões (CQRS/MediatR, value objects), integração pronta com RD Station, Twilio e SendGrid; tarefas assíncronas; documentação de deploy.
Pontos de melhoria: falta de autenticação/autorização multi‑tenant, ausência de testes automatizados, CI/CD incompleto, ausência de rate limiting/circuit breaker nos serviços externos, melhorias no frontend (dashboard e formulários), e documentação de variáveis de ambiente.

## Roadmap em 6 PRs

| Nº    | PR sugerido                               | Principais tarefas (resumidas)                                                                                                                                                                                                                                                                                                                                                                             |
| ----- | ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **1** | **Autenticação e segurança multi‑tenant** | Implementar autenticação (JWT ou [ASP.NET Identity](https://learn.microsoft.com)), associando cada usuário a um tenant. Criar middleware para validar o `TenantId` em cada requisição. Proteger endpoints e limitar acesso ao painel por tenant.                                                                                                                                                           |
| **2** | **Testes unitários e integração**         | Adicionar projetos de teste (xUnit). Cobrir entidades, value objects, handlers MediatR e serviços externos com mocks. Criar testes de integração para APIs de ingestão e worker. Configurar GitHub Actions para executar os testes.                                                                                                                                                                        |
| **3** | **CI/CD e deploy**                        | Criar workflow completo no GitHub Actions: checkout, `dotnet build` de todos os projetos, `npm ci` do frontend (gerando e versionando `package‑lock.json`), build das imagens Docker, execução dos testes e push para um registry. Ajustar step de Pages (`actions/configure-pages`) para ser opcional ou habilitar Pages no repo. Validar as instruções de `docs/DEPLOY.md` e automatizá-las no pipeline. |
| **4** | **Resiliência e observabilidade**         | Introduzir rate limiting e circuit breaker (ex. Polly) nas chamadas a RD Station, Twilio e OpenAI para evitar overloads. Implementar caching onde fizer sentido e centralizar logs com Serilog. Adicionar métricas (por exemplo, via Prometheus) e expandir `BackgroundJobProcessor` para retries configuráveis.                                                                                           |
| **5** | **Frontend e UX**                         | Finalizar e documentar o dashboard React: listar leads, mostrar status e histórico, configurar FAQ/hours per tenant, e formular de ingestão. Adicionar autenticação e gerenciamento de sessão. Incluir `package‑lock.json` no repo para garantir builds determinísticos e corrigir o problema com `npm ci`.                                                                                                |
| **6** | **Documentação e scripts de deploy**      | Atualizar README e `docs/DEPLOY.md`: explicar cada variável de ambiente, incluir um `.env.example`, detalhar configurações do Railway/MongoDB/Redis, e fornecer um script `docker-compose` funcional. Incluir instruções para habilitar GitHub Pages e checklist pós‑deploy (health checks, logs).                                                                                                         |

Esses PRs, organizados em ordem de dependência, cobrem os principais pontos necessários para que o **LeadFlowAI** fique pronto para produção, alinhando-se à arquitetura proposta e às boas práticas de software.