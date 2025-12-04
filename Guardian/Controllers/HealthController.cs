using Microsoft.AspNetCore.Mvc;

namespace Guardian.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    // GET api/v1/health
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Healthy", version = "1.0" });
    }
}
