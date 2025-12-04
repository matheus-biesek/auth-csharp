using Microsoft.AspNetCore.Mvc;

namespace Guardian.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    // GET api/v2/health
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "Healthy", version = "2.0", details = new { message = "v2 endpoint" } });
    }
}
