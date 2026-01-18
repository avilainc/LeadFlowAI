using LeadFlowAI.Application.Interfaces;
using PhoneNumbers;

namespace LeadFlowAI.Infrastructure.Services;

/// <summary>
/// Implementação concreta de normalização de telefone usando libphonenumber-csharp
/// </summary>
public class PhoneNormalizer : IPhoneNormalizer
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneNormalizer()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    /// <inheritdoc/>
    public string? NormalizeToE164(string? raw, string defaultRegion)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            var parsedNumber = _phoneUtil.Parse(raw, defaultRegion);
            
            // Validar se o número é válido
            if (!_phoneUtil.IsValidNumber(parsedNumber))
                return null;

            return _phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        }
        catch (NumberParseException)
        {
            // Número inválido ou não parseável
            return null;
        }
    }
}
