namespace Guardian.Services.Common;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string username, IEnumerable<string>? roles = null, int expirationMinutes = 15);
    string GenerateCsrfToken();
    string GenerateRefreshToken();
}
