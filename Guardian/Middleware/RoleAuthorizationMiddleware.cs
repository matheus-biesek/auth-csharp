using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Guardian.Middleware;

/// <summary>
/// RoleAuthorizationMiddleware: checks for a required role (via RequireRoleAttribute) on endpoint
/// and validates that the provisional principal has the role.
/// </summary>
public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleAuthorizationMiddleware> _logger;

    public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Read required role from endpoint metadata
        var requiredRole = context.GetEndpoint()?.Metadata.GetMetadata<RequireRoleAttribute>()?.Role;

        if (string.IsNullOrEmpty(requiredRole))
        {
            await _next(context);
            return;
        }

        // Ensure there is a principal set by previous middleware
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("RoleAuthorization: no authenticated principal for endpoint requiring role {Role}", requiredRole);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "unauthenticated" });
            return;
        }

        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Contains(requiredRole))
        {
            _logger.LogWarning("RoleAuthorization: user {User} lacks required role {Role}", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, requiredRole);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "forbidden" });
            return;
        }

        await _next(context);
    }
}
