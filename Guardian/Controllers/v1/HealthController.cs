using Microsoft.AspNetCore.Mvc;
using Guardian.Services.V1;
using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthServiceV1 _service;

    public HealthController(IHealthServiceV1 service)
    {
        _service = service;
    }

    // GET api/v1/health
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        HealthResponse res = await _service.GetHealthAsync();
        return Ok(res);
    }
}
