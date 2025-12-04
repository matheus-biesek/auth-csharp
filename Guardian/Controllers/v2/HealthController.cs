[# 0001]
using Microsoft.AspNetCore.Mvc;
using Guardian.Services;
using System.Threading.Tasks;
using Guardian.Models;

namespace Guardian.Controllers.v2;
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthServiceV2 _service;

    public HealthController(IHealthServiceV2 service)
    {
        _service = service;
    }

    // GET api/v2/health
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        HealthResponse res = await _service.GetHealthAsync();
        return Ok(res);
    }
}
