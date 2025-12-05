namespace Guardian.Models.Auth.v1;

/// <summary>
/// Resposta de sucesso ao registrar um novo usuário.
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// ID do usuário recém-criado.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário registrado.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem de sucesso.
    /// </summary>
    public string Message { get; set; } = "Usuário registrado com sucesso";
}
