namespace Guardian.Models.Auth.v1;

/// <summary>
/// Modelo de requisição para autenticação de usuário.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email ou nome de usuário para identificar o usuário.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário (será verificada contra o hash armazenado).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
