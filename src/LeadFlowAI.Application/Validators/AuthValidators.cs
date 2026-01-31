using FluentValidation;
using LeadFlowAI.Application.Commands;

namespace LeadFlowAI.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres");
    }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Matches(@"[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Matches(@"[0-9]").WithMessage("Senha deve conter pelo menos um número");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(50).WithMessage("Nome deve ter no máximo 50 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Sobrenome é obrigatório")
            .MaximumLength(50).WithMessage("Sobrenome deve ter no máximo 50 caracteres");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Nome da empresa é obrigatório")
            .MaximumLength(100).WithMessage("Nome da empresa deve ter no máximo 100 caracteres");

        RuleFor(x => x.TenantSlug)
            .NotEmpty().WithMessage("Slug da empresa é obrigatório")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens")
            .MaximumLength(50).WithMessage("Slug deve ter no máximo 50 caracteres");
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token é obrigatório");
    }
}