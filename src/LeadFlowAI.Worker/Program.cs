using LeadFlowAI.Infrastructure.Persistence;
using LeadFlowAI.Infrastructure.Repositories;
using LeadFlowAI.Infrastructure.Services;
using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Worker;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/leadflowai-worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue<int>("WORKER_CONCURRENCY", 5);
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LeadFlowAI.Application.Commands.IngestWebFormLeadCommand).Assembly));

// Repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ILeadEventRepository, LeadEventRepository>();

// Services
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRDStationService, RDStationService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddScoped<IBackgroundJobProcessor, BackgroundJobProcessor>();

// HttpClient
builder.Services.AddHttpClient<ILLMService, LLMService>();
builder.Services.AddHttpClient<IRDStationService, RDStationService>();

// Worker Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
