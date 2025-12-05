using FluentValidation;
using Guardian.Models.Auth.v1;

namespace Guardian.Validators;

/// <summary>
/// Validador para requisição de revogação de token (RevokeTokenRequest).
/// </summary>
public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256).WithMessage("Email não pode ter mais de 256 caracteres");
    }
}
