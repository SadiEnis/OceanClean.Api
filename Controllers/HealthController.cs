using Microsoft.AspNetCore.Mvc;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(
            new
            {
                status = "ok",
                service = "OceanClean.Api",
                timestamp = DateTime.UtcNow
            });
    }
}