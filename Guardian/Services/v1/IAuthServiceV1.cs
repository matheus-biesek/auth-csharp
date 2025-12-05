using Guardian.Models.Auth.v1;

namespace Guardian.Services.v1;

public interface IAuthServiceV1
{
    /// <summary>
    /// Registra um novo usuário no sistema (sem autenticação).
    /// </summary>
    Task<(bool success, RegisterResponse? response, string? error)> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Autentica usuário e retorna tokens separadamente para serem setados em cookies.
    /// </summary>
    Task<(bool success, string? accessToken, string? refreshToken, string? csrfToken, LoginResponse? response, string? error)> LoginAsync(LoginRequest request);

    /// <summary>
    /// Renova o access token usando o refresh token (com rotação de tokens).
    /// </summary>
    Task<(bool success, string? newAccessToken, string? newRefreshToken, string? newCsrfToken, string? error)> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoga o refresh token de um usuário (apenas admin).
    /// Remove tanto o token armazenado em Redis quanto o mapping de lookup.
    /// </summary>
    Task<(bool success, string? error)> RevokeRefreshTokenAsync(string email);
}
