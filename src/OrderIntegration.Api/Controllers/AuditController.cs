using Microsoft.AspNetCore.Mvc;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Audit;

namespace OrderIntegration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Obtiene eventos de auditoría con filtros.
    /// </summary>
    /// <param name="parameters">Parámetros de filtro.</param>
    /// <returns>Lista de eventos de auditoría.</returns>
    /// <response code="200">Lista de eventos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditEventResponse>>> GetEvents([FromQuery] AuditQueryParameters parameters)
    {
        var events = await _auditService.GetEventsAsync(parameters);
        return Ok(events);
    }

    /// <summary>
    /// Obtiene los eventos de auditoría más recientes.
    /// </summary>
    /// <param name="limit">Límite de resultados (máximo 500, por defecto 100).</param>
    /// <returns>Lista de eventos recientes.</returns>
    /// <response code="200">Lista de eventos recientes.</response>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditEventResponse>>> GetRecentEvents([FromQuery] int limit = 100)
    {
        if (limit > 500) limit = 500;
        if (limit < 1) limit = 1;

        var events = await _auditService.GetRecentEventsAsync(limit);
        return Ok(events);
    }
}
