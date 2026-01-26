using OrderIntegration.Api.Contracts.Integration;

namespace OrderIntegration.Api.Application.Interfaces;

/// <summary>
/// Servicio para integración con sistemas externos.
/// </summary>
public interface IIntegrationService
{
    /// <summary>
    /// Envía un pedido al ERP.
    /// </summary>
    /// <param name="orderId">ID del pedido.</param>
    /// <param name="correlationId">ID de correlación opcional.</param>
    /// <returns>Resultado del envío.</returns>
    Task<SendToErpResponse?> SendOrderToErpAsync(long orderId, string? correlationId = null);

    /// <summary>
    /// Procesa el webhook de confirmación del ERP.
    /// </summary>
    /// <param name="request">Datos del webhook.</param>
    /// <returns>Resultado del procesamiento.</returns>
    Task<ErpWebhookResponse> ProcessErpWebhookAsync(ErpWebhookRequest request);
}
