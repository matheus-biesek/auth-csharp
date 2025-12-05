using Guardian.Models.Auth.v1;

namespace Guardian.Services.v1;

public interface IAuthServiceV1
{
    // Returns tokens separately so controller can set cookies; LoginResponse does not contain tokens.
    Task<(bool success, string? accessToken, string? refreshToken, string? csrfToken, LoginResponse? response, string? error)> LoginAsync(LoginRequest request);
    Task<(bool success, string? newAccessToken, string? newRefreshToken, string? newCsrfToken, string? error)> RefreshTokenAsync(string refreshToken);
}
