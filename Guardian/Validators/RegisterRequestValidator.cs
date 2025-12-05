using FluentValidation;
using Guardian.Models.Auth.v1;

namespace Guardian.Validators;

/// <summary>
/// Validador para requisição de registro de usuário (RegisterRequest).
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256).WithMessage("Email não pode ter mais de 256 caracteres");

        // Username
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Nome de usuário é obrigatório")
            .Length(3, 50).WithMessage("Nome de usuário deve ter entre 3 e 50 caracteres")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Nome de usuário só pode conter letras, números, ponto, hífen e underscore");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres")
            .MaximumLength(100).WithMessage("Senha não pode ter mais de 100 caracteres");

        // PasswordConfirmation
        RuleFor(x => x.PasswordConfirmation)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
            .Equal(x => x.Password).WithMessage("As senhas não coincidem");
    }
}
