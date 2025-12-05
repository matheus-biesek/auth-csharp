using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get current user profile - requires authentication
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        _logger.LogInformation("Profile accessed by user {UserId}", userId);

        return Ok(new { userId, username, message = "Autenticação via framework ASP.NET Core" });
    }

    /// <summary>
    /// Get all users - requires Admin role
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAllUsers()
    {
        _logger.LogInformation("Admin accessed user list");

        return Ok(new
        {
            users = new[] { "user1@example.com", "user2@example.com" },
            message = "Autorização via framework ASP.NET Core"
        });
    }

    /// <summary>
    /// Delete user - requires Admin role
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult DeleteUser(string userId)
    {
        _logger.LogInformation("Admin deleted user {UserId}", userId);
        return NoContent();
    }
}
