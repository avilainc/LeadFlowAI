using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net.Http.Headers;

namespace LeadFlowAI.Infrastructure.Resilience;

public static class HttpClientResilienceExtensions
{
    /// <summary>
    /// Adiciona HttpClient configurado com políticas de resiliência
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null)
    {
        var builder = services.AddHttpClient(clientName, configureClient ?? (_ => { }));
        builder.AddStandardResilienceHandler();
        return builder;
    }

    /// <summary>
    /// Adiciona HttpClient para OpenAI com configuração específica
    /// </summary>
    public static IHttpClientBuilder AddOpenAIHttpClient(this IServiceCollection services)
    {
        return services.AddResilientHttpClient("OpenAI", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/v1/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(2); // OpenAI pode ser lento
        });
    }

    /// <summary>
    /// Adiciona HttpClient para Twilio com configuração específica
    /// </summary>
    public static IHttpClientBuilder AddTwilioHttpClient(this IServiceCollection services)
    {
        return services.AddResilientHttpClient("Twilio", client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }

    /// <summary>
    /// Adiciona HttpClient para SendGrid com configuração específica
    /// </summary>
    public static IHttpClientBuilder AddSendGridHttpClient(this IServiceCollection services)
    {
        return services.AddResilientHttpClient("SendGrid", client =>
        {
            client.BaseAddress = new Uri("https://api.sendgrid.com/v3/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }

    /// <summary>
    /// Adiciona HttpClient para RD Station com configuração específica
    /// </summary>
    public static IHttpClientBuilder AddRDStationHttpClient(this IServiceCollection services)
    {
        return services.AddResilientHttpClient("RDStation", client =>
        {
            client.BaseAddress = new Uri("https://api.rd.services/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}