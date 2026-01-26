using Microsoft.AspNetCore.Mvc;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Integration;

namespace OrderIntegration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IIntegrationService _integrationService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IIntegrationService integrationService, ILogger<WebhooksController> logger)
    {
        _integrationService = integrationService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para recibir confirmaciones del ERP (webhook).
    /// </summary>
    /// <param name="request">Datos de la confirmación del ERP.</param>
    /// <returns>Resultado del procesamiento.</returns>
    /// <response code="200">Webhook procesado correctamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    [HttpPost("erp/order-ack")]
    [ProducesResponseType(typeof(ErpWebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ErpWebhookResponse>> ErpOrderAck([FromBody] ErpWebhookRequest request)
    {
        _logger.LogInformation(
            "Webhook recibido del ERP para pedido {OrderNumber}. Success: {Success}",
            request.OrderNumber, request.Success);

        var result = await _integrationService.ProcessErpWebhookAsync(request);

        return Ok(result);
    }
}
