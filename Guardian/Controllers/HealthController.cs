using Microsoft.AspNetCore.Mvc;

namespace Guardian.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    // GET api/v1/health
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult Get()
    {
        return Ok(new { status = "Healthy", version = "1.0" });
    }

    // GET api/v2/health
    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2()
    {
        return Ok(new { status = "Healthy", version = "2.0", details = new { message = "v2 endpoint" } });
    }
}
