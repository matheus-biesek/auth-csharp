namespace Guardian.Models.Auth.v1;

/// <summary>
/// Modelo de requisição para registro de novo usuário no sistema.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Email do usuário (identificador único).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome de usuário único no sistema.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário (será hasheada no servidor).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmação da senha (deve coincidir com Password).
    /// </summary>
    public string PasswordConfirmation { get; set; } = string.Empty;
}
