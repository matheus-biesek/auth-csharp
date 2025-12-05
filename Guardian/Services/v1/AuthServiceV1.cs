using Guardian.Models.Auth.v1;
using Microsoft.AspNetCore.Identity;

namespace Guardian.Services.v1;

public class AuthServiceV1 : IAuthServiceV1
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly Guardian.Services.Common.ITokenService _tokenService;
    private readonly Guardian.Services.Common.IRedisService _redisService;
    private readonly ILogger<AuthServiceV1> _logger;

    public AuthServiceV1(
        UserManager<IdentityUser> userManager,
        Guardian.Services.Common.ITokenService tokenService,
        Guardian.Services.Common.IRedisService redisService,
        ILogger<AuthServiceV1> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<(bool success, string? accessToken, string? refreshToken, string? csrfToken, LoginResponse? response, string? error)> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
                return (false, null, null, null, null, "Credenciais inválidas");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
                return (false, null, null, null, null, "Credenciais inválidas");
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, expirationMinutes: 15);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var csrfToken = _tokenService.GenerateCsrfToken();

            // Store refresh token in Redis (7 days expiration) and keep a reverse lookup from token->userId
            await _redisService.SetAsync($"refresh_token:{user.Id}", refreshToken, TimeSpan.FromDays(7));
            await _redisService.SetAsync($"refresh_lookup:{refreshToken}", user.Id, TimeSpan.FromDays(7));

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            var response = new LoginResponse
            {
                ExpiresIn = 15 * 60, // 15 minutes in seconds
                TokenType = "Bearer"
            };

            // Return tokens separately; controller will set cookies and return only metadata in the body
            return (true, accessToken, refreshToken, csrfToken, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
            return (false, null, null, null, null, "Erro interno do servidor");
        }
    }

    public async Task<(bool success, string? newAccessToken, string? newRefreshToken, string? newCsrfToken, string? error)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Lookup user id from refresh token to support refresh when access token expired
            var userId = await _redisService.GetAsync($"refresh_lookup:{refreshToken}");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid refresh token attempt (no lookup for token)");
                return (false, null, null, null, "Token de renovação inválido");
            }

            var storedToken = await _redisService.GetAsync($"refresh_token:{userId}");
            if (storedToken != refreshToken)
            {
                _logger.LogWarning("Invalid refresh token attempt for user: {UserId}", userId);
                return (false, null, null, null, "Token de renovação inválido");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, null, null, null, "Usuário não encontrado");
            }

            // Generate new tokens (rotate refresh token)
            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, expirationMinutes: 15);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newCsrfToken = _tokenService.GenerateCsrfToken();

            // Store new refresh token and lookup mapping in Redis (7 days)
            await _redisService.SetAsync($"refresh_token:{user.Id}", newRefreshToken, TimeSpan.FromDays(7));
            await _redisService.SetAsync($"refresh_lookup:{newRefreshToken}", user.Id, TimeSpan.FromDays(7));

            // Remove old lookup key
            await _redisService.DeleteAsync($"refresh_lookup:{refreshToken}");

            _logger.LogInformation("Token refreshed and rotated for user: {UserId}", userId);

            return (true, newAccessToken, newRefreshToken, newCsrfToken, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return (false, null, null, null, "Erro ao renovar token");
        }
    }
}

