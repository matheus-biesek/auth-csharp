using Guardian.Models.Auth.v1;
using Guardian.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServiceV1 _authService;
    private readonly Guardian.Services.Common.IRedisService _redisService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthServiceV1 authService,
        Guardian.Services.Common.IRedisService redisService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _redisService = redisService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user in the system (no authentication required).
    /// </summary>
    /// <param name="request">Registration data (email, username, password)</param>
    /// <returns>User ID and success message</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, response, error) = await _authService.RegisterAsync(request);

        if (!success)
        {
            _logger.LogWarning("Registration failed: {Error}", error);

            // Conflict se email ou username já existem
            if (error!.Contains("já registrado") || error.Contains("já está em uso"))
            {
                return Conflict(new { message = error });
            }

            return BadRequest(new { message = error });
        }

        _logger.LogInformation("User registered successfully: {UserId}", response!.UserId);

        return CreatedAtAction(nameof(Register), new { userId = response.UserId }, response);
    }

    /// <summary>
    /// Authenticate user and return access token, CSRF token, and refresh token (via HttpOnly cookie).
    /// </summary>
    /// <param name="request">Login credentials (username and password)</param>
    /// <returns>Access token and CSRF token in response body, refresh token in HttpOnly cookie</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, accessToken, refreshToken, csrfToken, response, error) = await _authService.LoginAsync(request);

        if (!success)
        {
            _logger.LogWarning("Login failed: {Error}", error);
            return Unauthorized(new { message = error });
        }

        // Set access token as HttpOnly cookie (short-lived)
        Response.Cookies.Append("accessToken", accessToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        // Set refresh token as HttpOnly cookie (7 days)
        Response.Cookies.Append("refreshToken", refreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        // Set CSRF token as a cookie accessible to JavaScript (HttpOnly = false)
        Response.Cookies.Append("csrfToken", csrfToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        _logger.LogInformation("User login successful");

        // Return only metadata in body; tokens live in cookies
        return Ok(response);
    }

    /// <summary>
    /// Refresh the access token using the refresh token from HttpOnly cookie.
    /// Refresh token não é validado pelo framework, possui validação própria.
    /// </summary>
    /// <returns>New access token and CSRF token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized(new { message = "Refresh token não encontrado" });
        }

        var (success, newAccessToken, newRefreshToken, newCsrf, error) = await _authService.RefreshTokenAsync(refreshToken);

        if (!success)
        {
            _logger.LogWarning("Token refresh failed: {Error}", error);
            return Unauthorized(new { message = error });
        }

        // Set cookies for the new tokens
        Response.Cookies.Append("accessToken", newAccessToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refreshToken", newRefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        Response.Cookies.Append("csrfToken", newCsrf!, new CookieOptions
        {
            HttpOnly = false,
            Secure = !HttpContext.Connection.LocalIpAddress?.ToString().Contains("127") != true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        return Ok(new { tokenType = "Bearer", expiresIn = 15 * 60 });
    }

    /// <summary>
    /// Logout user by invalidating refresh token.
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        // Delete refresh token mapping and cookies
        var refreshToken = Request.Cookies.TryGetValue("refreshToken", out var cookie) ? cookie : null;
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await _redisService.DeleteAsync($"refresh_token:{userId}");
        }

        if (!string.IsNullOrEmpty(refreshToken))
        {
            // attempt to find mapped user and remove both sides
            var mapped = await _redisService.GetAsync($"refresh_lookup:{refreshToken}");
            if (!string.IsNullOrEmpty(mapped))
            {
                await _redisService.DeleteAsync($"refresh_token:{mapped}");
            }

            await _redisService.DeleteAsync($"refresh_lookup:{refreshToken}");
        }

        Response.Cookies.Delete("refreshToken");
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("csrfToken");

        _logger.LogInformation("User logged out");

        return NoContent();
    }
}
