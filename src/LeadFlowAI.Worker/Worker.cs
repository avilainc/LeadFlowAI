namespace LeadFlowAI.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeadFlowAI Worker iniciado em: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Worker rodando em: {time}", DateTimeOffset.Now);
            await Task.Delay(60000, stoppingToken); // Check a cada 1 minuto
        }

        _logger.LogInformation("LeadFlowAI Worker parando em: {time}", DateTimeOffset.Now);
    }
}
