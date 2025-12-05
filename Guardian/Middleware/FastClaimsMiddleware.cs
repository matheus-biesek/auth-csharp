using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Guardian.Middleware;

/// <summary>
/// FastClaimsMiddleware: lightweight middleware that decodes JWT payload (no signature check)
/// and verifies the `exp` claim. If valid, it populates HttpContext.User with a provisional principal.
/// </summary>
public class FastClaimsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FastClaimsMiddleware> _logger;

    public FastClaimsMiddleware(RequestDelegate next, ILogger<FastClaimsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        // also support access token passed as a cookie when front-end uses cookie-based tokens
        if (string.IsNullOrEmpty(authHeader) && context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
        {
            authHeader = $"Bearer {cookieToken}";
        }
        if (string.IsNullOrEmpty(authHeader))
        {
            await _next(context);
            return;
        }

        try
        {
            var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                await _next(context);
                return;
            }

            var payloadJson = Base64Decode(parts[1]);
            var claimsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
            if (claimsDict == null)
            {
                await _next(context);
                return;
            }

            // Expiration check (exp) - numeric seconds since epoch
            if (claimsDict.TryGetValue("exp", out var expObj) && long.TryParse(expObj.ToString(), out var expTs))
            {
                var expiration = UnixTimeStampToDateTime(expTs);
                if (DateTime.UtcNow > expiration)
                {
                    _logger.LogDebug("FastClaims: token expired at {Expiration}", expiration);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "token_expired" });
                    return;
                }
            }

            // Build identity from available claims
            var identity = new ClaimsIdentity("FastClaims");
            if (claimsDict.TryGetValue("sub", out var sub) && sub != null)
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub.ToString() ?? string.Empty));
            if (claimsDict.TryGetValue("email", out var email) && email != null)
                identity.AddClaim(new Claim(ClaimTypes.Email, email.ToString() ?? string.Empty));
            if (claimsDict.TryGetValue("roles", out var rolesObj) && rolesObj != null)
            {
                // roles claim may be array or string
                if (rolesObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in je.EnumerateArray())
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, r.GetString() ?? string.Empty));
                    }
                }
                else
                {
                    var rolesStr = rolesObj.ToString();
                    if (!string.IsNullOrEmpty(rolesStr))
                    {
                        foreach (var r in rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, r.Trim()));
                        }
                    }
                }
            }

            var principal = new ClaimsPrincipal(identity);
            context.User = principal;

            _logger.LogDebug("FastClaims: provisional principal set for request");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "FastClaims: could not parse token, skipping");
        }

        await _next(context);
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
        return dateTime;
    }

    private static string Base64Decode(string base64EncodedData)
    {
        try
        {
            var padded = base64EncodedData.PadRight(base64EncodedData.Length + (4 - base64EncodedData.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(padded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            throw new SecurityTokenException("Invalid base64 in token payload");
        }
    }
}
