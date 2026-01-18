using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LeadFlowAI.Infrastructure.Services;

public class RDStationService : IRDStationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RDStationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> CreateOrUpdateLeadAsync(Lead lead, Tenant tenant, CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(tenant, cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var payload = new
            {
                email = lead.Email ?? $"{lead.Id}@leadflowai.placeholder.com",
                name = lead.Name,
                mobile_phone = lead.PhoneNormalized ?? lead.Phone,
                personal_phone = lead.Phone,
                city = lead.City,
                state = lead.State,
                company = lead.Company,
                cf_lead_score = lead.LeadScore,
                cf_intent = lead.Intent?.ToString(),
                cf_urgency = lead.Urgency?.ToString(),
                cf_lead_id = lead.Id.ToString(),
                tags = BuildTags(lead)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.rd.services/platform/contacts",
                payload,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao sincronizar com RD Station: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateLeadTagsAsync(string externalId, List<string> tags, Tenant tenant, CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync(tenant, cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
                return false;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var payload = new { tags = tags };

            var response = await _httpClient.PatchAsync(
                $"https://api.rd.services/platform/contacts/{externalId}",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string?> GetAccessTokenAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        // Verificar se token ainda é válido
        if (!string.IsNullOrEmpty(tenant.RDStationAccessToken) && 
            tenant.RDStationTokenExpiresAt.HasValue && 
            tenant.RDStationTokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return tenant.RDStationAccessToken;
        }

        // Refresh token
        if (string.IsNullOrEmpty(tenant.RDStationRefreshToken))
            return null;

        try
        {
            var clientId = tenant.RDStationClientId ?? _configuration["RDStation:ClientId"];
            var clientSecret = tenant.RDStationClientSecret ?? _configuration["RDStation:ClientSecret"];

            var payload = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                refresh_token = tenant.RDStationRefreshToken
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.rd.services/auth/token",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<RDTokenResponse>(cancellationToken: cancellationToken);
            
            // Atualizar tenant com novos tokens (isso deve ser feito no repositório)
            tenant.RDStationAccessToken = result?.AccessToken;
            tenant.RDStationRefreshToken = result?.RefreshToken;
            tenant.RDStationTokenExpiresAt = DateTime.UtcNow.AddSeconds(result?.ExpiresIn ?? 3600);

            return result?.AccessToken;
        }
        catch
        {
            return null;
        }
    }

    private List<string> BuildTags(Lead lead)
    {
        var tags = new List<string>();

        if (lead.LeadScore.HasValue)
        {
            if (lead.LeadScore >= 80) tags.Add("lead-hot");
            else if (lead.LeadScore >= 50) tags.Add("lead-warm");
            else tags.Add("lead-cold");
        }

        if (lead.Intent.HasValue)
            tags.Add($"intent-{lead.Intent.Value.ToString().ToLower()}");

        if (lead.Urgency.HasValue)
            tags.Add($"urgency-{lead.Urgency.Value.ToString().ToLower()}");

        tags.Add($"source-{lead.Source.ToString().ToLower()}");

        return tags;
    }

    private class RDTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
