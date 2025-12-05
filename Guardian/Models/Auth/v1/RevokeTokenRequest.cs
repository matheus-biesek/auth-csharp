namespace Guardian.Models.Auth.v1;

/// <summary>
/// Modelo de requisição para revogar refresh token de um usuário.
/// Apenas administradores podem usar este endpoint.
/// </summary>
public class RevokeTokenRequest
{
    /// <summary>
    /// Email do usuário cujo refresh token será revogado.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
