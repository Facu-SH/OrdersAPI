using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Infrastructure.Persistence;
using System.Reflection;

namespace OrderIntegration.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check básico (para load balancers y Docker)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var response = new HealthResponse
        {
            Timestamp = DateTime.UtcNow,
            Version = GetVersion()
        };

        try
        {
            // Verificar conexión a la base de datos
            var canConnect = await _context.Database.CanConnectAsync();
            response.Database = canConnect ? "Healthy" : "Unhealthy";
            response.Status = canConnect ? "Healthy" : "Degraded";

            if (!canConnect)
            {
                _logger.LogWarning("Health check: No se puede conectar a la base de datos");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Error al verificar la base de datos");
            response.Status = "Unhealthy";
            response.Database = "Error";
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Health check detallado con información adicional
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailed()
    {
        var response = new DetailedHealthResponse
        {
            Timestamp = DateTime.UtcNow,
            Version = GetVersion(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            MachineName = Environment.MachineName
        };

        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            response.Database = canConnect ? "Healthy" : "Unhealthy";
            response.Status = canConnect ? "Healthy" : "Degraded";

            if (canConnect)
            {
                // Obtener estadísticas básicas
                response.OrderCount = await _context.Orders.CountAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            response.Status = "Unhealthy";
            response.Database = "Error";
        }

        return Ok(response);
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}

public record HealthResponse
{
    public string Status { get; set; } = "Unknown";
    public string Database { get; set; } = "Unknown";
    public string Version { get; set; } = "Unknown";
    public DateTime Timestamp { get; init; }
}

public record DetailedHealthResponse : HealthResponse
{
    public string Environment { get; set; } = "Unknown";
    public string MachineName { get; set; } = "Unknown";
    public int? OrderCount { get; set; }
}
