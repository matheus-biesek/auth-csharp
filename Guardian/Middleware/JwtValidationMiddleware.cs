using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Guardian.Services.Common;

namespace Guardian.Middleware;

/// <summary>
/// JwtValidationMiddleware: performs full cryptographic validation of the JWT and sets a final principal.
/// Uses ITokenService.ValidateToken under the hood.
/// </summary>
public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, Guardian.Services.Common.ITokenService tokenService)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        // support cookie-based access token if Authorization header isn't present
        if (string.IsNullOrEmpty(authHeader) && context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
        {
            authHeader = $"Bearer {cookieToken}";
        }
        if (string.IsNullOrEmpty(authHeader))
        {
            await _next(context);
            return;
        }

        var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        try
        {
            var principal = tokenService.ValidateToken(token);
            if (principal == null)
            {
                _logger.LogDebug("JwtValidation: invalid token signature or claims");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "invalid_token" });
                return;
            }

            // Set final principal
            context.User = principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JwtValidation: unexpected error during token validation");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "invalid_token" });
            return;
        }

        await _next(context);
    }
}
