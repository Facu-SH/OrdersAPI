using Microsoft.AspNetCore.Mvc;

namespace OrderIntegration.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}

public record HealthResponse
{
    public string Status { get; init; } = default!;
    public DateTime Timestamp { get; init; }
}
