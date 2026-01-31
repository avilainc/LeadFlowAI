using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.CircuitBreaker;

namespace LeadFlowAI.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    /// <summary>
    /// Política de retry para operações HTTP
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Serilog.Log.Warning(
                        "HTTP request failed. Retrying in {RetryTimeSpan}. Attempt {RetryAttempt}/{MaxRetries}. " +
                        "Context: {@Context}. Exception: {Exception}",
                        timespan,
                        retryAttempt,
                        3,
                        context,
                        outcome.Exception?.Message);
                });
    }

    /// <summary>
    /// Política de circuit breaker para operações HTTP
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, timespan) =>
                {
                    Serilog.Log.Warning(
                        "Circuit breaker opened for {BreakDuration} due to {Exception}",
                        timespan,
                        outcome.Exception?.Message);
                },
                onReset: () =>
                {
                    Serilog.Log.Information("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Serilog.Log.Information("Circuit breaker half-open, testing next request");
                });
    }

    /// <summary>
    /// Política combinada de retry + circuit breaker para HTTP
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpResiliencePolicy()
    {
        return Policy.WrapAsync(
            GetHttpRetryPolicy(),
            GetHttpCircuitBreakerPolicy(),
            GetHttpTimeoutPolicy());
    }

    /// <summary>
    /// Política de timeout para operações HTTP
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(30),
            onTimeoutAsync: (context, timespan, task) =>
            {
                Serilog.Log.Warning(
                    "HTTP request timed out after {Timeout}. Context: {@Context}",
                    timespan,
                    context);
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Política de retry genérica para operações de banco
    /// </summary>
    public static AsyncRetryPolicy GetDatabaseRetryPolicy()
    {
        return Policy
            .Handle<Exception>(ex =>
                ex.GetType().Name.Contains("SqlException") ||
                ex.GetType().Name.Contains("DbUpdateException") ||
                ex.Message.Contains("connection") ||
                ex.Message.Contains("timeout"))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt),
                onRetry: (exception, timespan, retryAttempt, context) =>
                {
                    Serilog.Log.Warning(
                        "Database operation failed. Retrying in {RetryTimeSpan}. Attempt {RetryAttempt}/{MaxRetries}. " +
                        "Exception: {Exception}",
                        timespan,
                        retryAttempt,
                        3,
                        exception.Message);
                });
    }

    /// <summary>
    /// Política de retry para operações de cache (Redis)
    /// </summary>
    public static AsyncRetryPolicy GetCacheRetryPolicy()
    {
        return Policy
            .Handle<Exception>(ex =>
                ex.GetType().Name.Contains("Redis") ||
                ex.Message.Contains("connection") ||
                ex.Message.Contains("timeout"))
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(50 * retryAttempt),
                onRetry: (exception, timespan, retryAttempt, context) =>
                {
                    Serilog.Log.Warning(
                        "Cache operation failed. Retrying in {RetryTimeSpan}. Attempt {RetryAttempt}/{MaxRetries}. " +
                        "Exception: {Exception}",
                        timespan,
                        retryAttempt,
                        2,
                        exception.Message);
                });
    }
}