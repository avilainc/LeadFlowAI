using LeadFlowAI.Application.Interfaces;
using LeadFlowAI.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LeadFlowAI.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILeadRepository _leadRepository;
    private readonly ITenantRepository _tenantRepository;

    public LLMService(HttpClient httpClient, IConfiguration configuration, ILeadRepository leadRepository, ITenantRepository tenantRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _leadRepository = leadRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<string> QualifyLeadAsync(Guid leadId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);
        if (lead == null) throw new Exception("Lead não encontrado");

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new Exception("Tenant não encontrado");

        // Montar prompt do sistema
        var systemPrompt = BuildSystemPrompt(tenant);

        // Montar contexto do lead
        var userPrompt = BuildUserPrompt(lead, tenant);

        // Chamar OpenAI API
        var apiKey = _configuration["LLM:ApiKey"] ?? _configuration["OPENAI_API_KEY"];
        var model = _configuration["LLM:Model"] ?? "gpt-4";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.3,
            max_tokens = 1500,
            response_format = new { type = "json_object" }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.openai.com/v1/chat/completions",
            requestBody,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
        var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";

        // Validar JSON
        try
        {
            JsonDocument.Parse(content);
            return content;
        }
        catch
        {
            // Se não for JSON válido, tentar extrair
            return ExtractJsonFromText(content);
        }
    }

    private string BuildSystemPrompt(Tenant tenant)
    {
        return $@"Você é um SDR (Sales Development Representative) especializado em qualificar leads para {tenant.Name}.

**OBJETIVO**: Analisar o lead e retornar SOMENTE um JSON válido com a qualificação.

**CONTEXTO DO NEGÓCIO**:
- Serviços: {string.Join(", ", tenant.Config.Services)}
- Regiões atendidas: {string.Join(", ", tenant.Config.Regions)}
- Preço mínimo: {tenant.Config.MinimumPrice:C}
- Tom de voz: {tenant.Config.ToneOfVoice}

**PLAYBOOK**:
{tenant.Config.Playbook}

**FAQs**:
{string.Join("\n", tenant.Config.FAQs.Select(f => $"Q: {f.Question}\nA: {f.Answer}"))}

**RESTRIÇÕES**:
1. NUNCA pedir dados sensíveis (CPF, RG, senha, cartão de crédito)
2. Ser breve e objetivo (máximo 200 palavras)
3. Usar português brasileiro
4. Incluir sempre um CTA (call-to-action)
5. Se detectar dados sensíveis na mensagem, adicionar 'dados_sensiveis' em risk_flags

**SCHEMA JSON OBRIGATÓRIO**:
{{
  ""lead_score"": 0-100,
  ""intent"": ""orcamento|duvida|suporte|parceria|carreira|outro"",
  ""urgency"": ""baixa|media|alta"",
  ""service_match"": [""array de serviços""],
  ""key_details"": [""detalhes importantes identificados""],
  ""missing_questions"": [""perguntas que ainda precisam ser feitas""],
  ""risk_flags"": [""spam_suspeito|linguagem_abusiva|dados_sensiveis|fraude_suspeita""],
  ""recommended_next_step"": ""responder|perguntar|handoff|ignorar"",
  ""reply_channel"": ""whatsapp|email"",
  ""reply_message"": ""texto da resposta pronto para envio"",
  ""handoff_reason"": ""motivo para encaminhar para humano ou null""
}}

Retorne APENAS o JSON, sem markdown ou explicações adicionais.";
    }

    private string BuildUserPrompt(Lead lead, Tenant tenant)
    {
        return $@"Qualifique este lead:

**DADOS DO LEAD**:
- Nome: {lead.Name}
- Telefone: {lead.Phone}
- Email: {lead.Email ?? "não informado"}
- Empresa: {lead.Company ?? "não informado"}
- Cidade/Estado: {lead.City ?? ""} / {lead.State ?? ""}
- Mensagem: {lead.Message}

**ORIGEM**:
- Fonte: {lead.Source}
- URL: {lead.SourceUrl ?? ""}
- UTM Source: {lead.UtmSource ?? ""}
- UTM Campaign: {lead.UtmCampaign ?? ""}
- UTM Medium: {lead.UtmMedium ?? ""}

Retorne o JSON com a qualificação:";
    }

    private string ExtractJsonFromText(string text)
    {
        // Tentar extrair JSON de markdown ou texto
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        
        if (start >= 0 && end > start)
        {
            return text.Substring(start, end - start + 1);
        }

        throw new Exception("Não foi possível extrair JSON válido da resposta da LLM");
    }

    private class OpenAIResponse
    {
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}
