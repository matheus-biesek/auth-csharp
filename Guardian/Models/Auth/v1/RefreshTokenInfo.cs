namespace Guardian.Models.Auth.v1;

/// <summary>
/// Representa o email de um usuário que possui um refresh token ativo.
/// Apenas o email é exposto por motivos de segurança e privacidade.
/// </summary>
public class RefreshTokenInfo
{
    /// <summary>
    /// Email do usuário com refresh token ativo.
    /// </summary>
    public string Email { get; set; } = null!;
}
