using System.Security.Claims;
using Guardian.Services.Common;
using Microsoft.AspNetCore.Http;

namespace Guardian.Middleware;

/// <summary>
/// Middleware simples de rate limiting (fixed window) usando Redis para armazenar contadores.
/// - Identificador: usuário autenticado (`sub`/NameIdentifier) ou IP remoto quando anônimo.
/// - Configurável via configuração `RateLimiting` (RequestsPerWindow, WindowSeconds).
/// </summary>
public class GuardianRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GuardianRateLimitMiddleware> _logger;
    private readonly IRedisService _redisService;
    private readonly int _requestsPerWindow;
    private readonly int _windowSeconds;

    public GuardianRateLimitMiddleware(RequestDelegate next, ILogger<GuardianRateLimitMiddleware> logger, IRedisService redisService, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _redisService = redisService;

        _requestsPerWindow = configuration.GetValue<int?>("RateLimiting:RequestsPerWindow") ?? 60;
        _windowSeconds = configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Não aplicar rate limit para endpoints locais de health ou swagger (ajuste conforme necessário)
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger") || path.StartsWith("/health") || path.StartsWith("/metrics"))
        {
            await _next(context);
            return;
        }

        // Identificador por usuário autenticado ou por IP
        string identifier = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown";

        var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var window = utcNow / _windowSeconds; // fixed window index
        var key = $"ratelimit:{identifier}:{window}";

        // Incrementar contador atomically e setar TTL para o final da janela
        var ttl = TimeSpan.FromSeconds(_windowSeconds);
        var count = await _redisService.IncrementAsync(key, 1, ttl);

        if (count < 0)
        {
            // Falha ao acessar Redis — permitir para evitar cortar tráfego (ou você pode optar por negar)
            _logger.LogWarning("Redis unavailable for rate limiting, allowing request");
            await _next(context);
            return;
        }

        if (count > _requestsPerWindow)
        {
            // Excedeu
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = _windowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new { message = "Too many requests" });
            _logger.LogInformation("Rate limit exceeded for {Identifier}: {Count}/{Limit}", identifier, count, _requestsPerWindow);
            return;
        }

        await _next(context);
    }
}
