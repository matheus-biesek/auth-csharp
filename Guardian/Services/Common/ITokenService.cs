using System.Security.Claims;

namespace Guardian.Services.Common;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, int expirationMinutes = 15);
    string GenerateCsrfToken();
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
