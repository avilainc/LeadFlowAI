using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace LeadFlowAI.WebAPI.Configuration;

public static class PrometheusConfiguration
{
    public static IServiceCollection AddPrometheusMetrics(this IServiceCollection services)
    {
        // Adiciona métricas padrão do ASP.NET Core
        services.AddMetrics();

        // Adiciona métricas customizadas
        var metricsRegistry = Metrics.WithCustomRegistry(new CollectorRegistry());

        // Métricas de negócio
        Metrics.CreateCounter("leadflowai_leads_ingested_total",
            "Total number of leads ingested",
            new CounterConfiguration
            {
                LabelNames = new[] { "source", "tenant_id" }
            });

        Metrics.CreateCounter("leadflowai_leads_qualified_total",
            "Total number of leads qualified",
            new CounterConfiguration
            {
                LabelNames = new[] { "result", "tenant_id" }
            });

        Metrics.CreateCounter("leadflowai_emails_sent_total",
            "Total number of emails sent",
            new CounterConfiguration
            {
                LabelNames = new[] { "status", "tenant_id" }
            });

        Metrics.CreateCounter("leadflowai_whatsapp_messages_sent_total",
            "Total number of WhatsApp messages sent",
            new CounterConfiguration
            {
                LabelNames = new[] { "status", "tenant_id" }
            });

        Metrics.CreateHistogram("leadflowai_llm_request_duration_seconds",
            "Duration of LLM requests in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "operation", "status" },
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
            });

        Metrics.CreateHistogram("leadflowai_external_api_request_duration_seconds",
            "Duration of external API requests in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "service", "operation", "status" },
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
            });

        return services;
    }

    public static WebApplication UsePrometheusMetrics(this WebApplication app)
    {
        // Endpoint para métricas do Prometheus
        app.MapMetrics("/metrics");

        // Middleware para coletar métricas HTTP
        app.UseHttpMetrics(options =>
        {
            options.AddCustomLabel("tenant", context =>
            {
                // Extrair tenant do contexto (JWT claim, header, etc.)
                return context.User.FindFirst("tenant_id")?.Value ?? "unknown";
            });
        });

        return app;
    }
}