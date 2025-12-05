using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Guardian.Middleware;

/// <summary>
/// CsrfValidationMiddleware: valida o CSRF token em requisições sensíveis (POST, PUT, DELETE, PATCH).
/// Compara o token do cookie com o token enviado no header X-CSRF-Token.
/// </summary>
public class CsrfValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfValidationMiddleware> _logger;
    private static readonly string[] SensitiveMethods = { "POST", "PUT", "DELETE", "PATCH" };

    public CsrfValidationMiddleware(RequestDelegate next, ILogger<CsrfValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Apenas valida CSRF em requisições sensíveis
        if (!SensitiveMethods.Contains(context.Request.Method.ToUpperInvariant()))
        {
            await _next(context);
            return;
        }

        // CSRF validation só faz sentido para requisições autenticadas
        // Se o usuário não estiver autenticado, pula a validação CSRF
        // (a validação de autenticação já foi feita pelo UseAuthentication() do framework)
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Verifica se há um CSRF token no cookie
        if (!context.Request.Cookies.TryGetValue("csrfToken", out var cookieToken) || string.IsNullOrEmpty(cookieToken))
        {
            _logger.LogWarning("CSRF validation failed: no CSRF token in cookie for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "CSRF token não encontrado" });
            return;
        }

        // Verifica se há um CSRF token no header
        var headerToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault() 
            ?? context.Request.Headers["X-XSRF-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(headerToken))
        {
            _logger.LogWarning("CSRF validation failed: no CSRF token in header for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "CSRF token não encontrado no header" });
            return;
        }

        // Compara os tokens (comparação segura contra timing attacks)
        if (!SecureCompare(cookieToken, headerToken))
        {
            _logger.LogWarning("CSRF validation failed: token mismatch for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "CSRF token inválido" });
            return;
        }

        _logger.LogDebug("CSRF validation passed for {Method} {Path}", context.Request.Method, context.Request.Path);
        await _next(context);
    }

    /// <summary>
    /// Comparação segura de strings para prevenir timing attacks.
    /// </summary>
    private static bool SecureCompare(string a, string b)
    {
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

