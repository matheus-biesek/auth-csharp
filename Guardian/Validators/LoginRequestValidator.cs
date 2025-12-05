using FluentValidation;
using Guardian.Models.Auth.v1;

namespace Guardian.Validators;

/// <summary>
/// Validador para requisição de login (LoginRequest).
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256).WithMessage("Email não pode ter mais de 256 caracteres");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}
