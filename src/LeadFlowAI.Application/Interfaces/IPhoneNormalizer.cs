namespace LeadFlowAI.Application.Interfaces;

/// <summary>
/// Interface para normalização de números de telefone para formato E.164
/// </summary>
public interface IPhoneNormalizer
{
    /// <summary>
    /// Normaliza um número de telefone para o formato E.164
    /// </summary>
    /// <param name="raw">Número de telefone em formato raw (com ou sem formatação)</param>
    /// <param name="defaultRegion">Código da região padrão (ex: "BR", "PT", "US")</param>
    /// <returns>Número normalizado no formato E.164 ou null se inválido</returns>
    string? NormalizeToE164(string? raw, string defaultRegion);
}
