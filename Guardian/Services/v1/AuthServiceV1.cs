using Guardian.Models;
using Guardian.Models.Auth.v1;
using Microsoft.AspNetCore.Identity;

namespace Guardian.Services.v1;

public class AuthServiceV1 : IAuthServiceV1
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly Guardian.Services.Common.ITokenService _tokenService;
    private readonly Guardian.Services.Common.IRedisService _redisService;
    private readonly ILogger<AuthServiceV1> _logger;

    public AuthServiceV1(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        Guardian.Services.Common.ITokenService tokenService,
        Guardian.Services.Common.IRedisService redisService,
        ILogger<AuthServiceV1> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<(bool success, string? accessToken, string? refreshToken, string? csrfToken, LoginResponse? response, string? error)> LoginAsync(LoginRequest request)
    {
        try
        {
            // Busca usuário por username (Email será buscado depois)
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", request.Username);
                return (false, null, null, null, null, "Credenciais inválidas");
            }

            // Valida se usuário está ativo
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Username}", request.Username);
                return (false, null, null, null, null, "Usuário inativo");
            }

            // Valida senha
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return (false, null, null, null, null, "Credenciais inválidas");
            }

            // Recupera roles do usuário via Identity Framework
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesArray = userRoles.ToArray();

            // Gera tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, rolesArray, expirationMinutes: 15);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var csrfToken = _tokenService.GenerateCsrfToken();

            // Armazena refresh token em Redis com lookup reverso (7 dias)
            await _redisService.SetAsync($"refresh_token:{user.Id}", refreshToken, TimeSpan.FromDays(7));
            await _redisService.SetAsync($"refresh_lookup:{refreshToken}", user.Id, TimeSpan.FromDays(7));

            _logger.LogInformation("User logged in successfully: {UserId} ({Username})", user.Id, user.UserName);

            var response = new LoginResponse
            {
                ExpiresIn = 15 * 60, // 15 minutos em segundos
                TokenType = "Bearer"
            };

            // Retorna tokens separadamente; controller coloca em cookies e retorna apenas metadata
            return (true, accessToken, refreshToken, csrfToken, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return (false, null, null, null, null, "Erro interno do servidor");
        }
    }

    public async Task<(bool success, string? newAccessToken, string? newRefreshToken, string? newCsrfToken, string? error)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Busca ID do usuário pelo refresh token
            var userId = await _redisService.GetAsync($"refresh_lookup:{refreshToken}");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid refresh token attempt (no lookup for token)");
                return (false, null, null, null, "Token de renovação inválido");
            }

            // Valida se o refresh token está armazenado
            var storedToken = await _redisService.GetAsync($"refresh_token:{userId}");
            if (storedToken != refreshToken)
            {
                _logger.LogWarning("Invalid refresh token attempt for user: {UserId}", userId);
                return (false, null, null, null, "Token de renovação inválido");
            }

            // Recupera usuário
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found during refresh: {UserId}", userId);
                return (false, null, null, null, "Usuário não encontrado");
            }

            // Valida se usuário está ativo
            if (!user.IsActive)
            {
                _logger.LogWarning("Refresh attempt for inactive user: {UserId}", userId);
                return (false, null, null, null, "Usuário inativo");
            }

            // Recupera roles atualizadas do usuário
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesArray = userRoles.ToArray();

            // Gera novos tokens (rotaciona refresh token)
            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, rolesArray, expirationMinutes: 15);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newCsrfToken = _tokenService.GenerateCsrfToken();

            // Atualiza mapping em Redis (7 dias)
            await _redisService.SetAsync($"refresh_token:{user.Id}", newRefreshToken, TimeSpan.FromDays(7));
            await _redisService.SetAsync($"refresh_lookup:{newRefreshToken}", user.Id, TimeSpan.FromDays(7));

            // Remove lookup antigo
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
