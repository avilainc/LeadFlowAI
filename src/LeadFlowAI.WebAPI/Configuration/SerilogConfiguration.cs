using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace LeadFlowAI.WebAPI.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration, IHostEnvironment environment)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "LeadFlowAI")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/leadflowai-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        // Configuração específica para desenvolvimento
        if (environment.IsDevelopment())
        {
            loggerConfiguration.MinimumLevel.Debug();
        }

        // Configuração para produção com Elasticsearch (se configurado)
        var elasticsearchUrl = configuration["Elasticsearch:Url"];
        if (!string.IsNullOrEmpty(elasticsearchUrl))
        {
            loggerConfiguration.WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
                {
                    IndexFormat = "leadflowai-logs-{0:yyyy.MM.dd}",
                    AutoRegisterTemplate = true,
                    NumberOfShards = 2,
                    NumberOfReplicas = 1,
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureCallback = (logEvent, ex) => Console.WriteLine($"Elasticsearch sink failure: {ex?.Message}")
                });
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    public static void AddRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                if (elapsed > 10000) // 10 segundos
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });
    }
}