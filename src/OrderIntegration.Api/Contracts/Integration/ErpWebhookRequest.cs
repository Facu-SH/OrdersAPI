using System.ComponentModel.DataAnnotations;

namespace OrderIntegration.Api.Contracts.Integration;

/// <summary>
/// Request del webhook de confirmación del ERP.
/// </summary>
public class ErpWebhookRequest
{
    /// <summary>
    /// Número del pedido.
    /// </summary>
    [Required(ErrorMessage = "El número de pedido es requerido.")]
    public string OrderNumber { get; set; } = default!;

    /// <summary>
    /// Indica si el ERP procesó exitosamente el pedido.
    /// </summary>
    [Required(ErrorMessage = "El campo success es requerido.")]
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje del ERP.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Referencia asignada por el ERP.
    /// </summary>
    public string? ErpReference { get; set; }

    /// <summary>
    /// ID de correlación para rastreo.
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Respuesta del procesamiento del webhook.
/// </summary>
public class ErpWebhookResponse
{
    public bool Processed { get; set; }
    public string Message { get; set; } = default!;
    public long? IntegrationAttemptId { get; set; }
    public string? OrderNumber { get; set; }
}
