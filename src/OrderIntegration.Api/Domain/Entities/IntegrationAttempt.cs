using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Domain.Entities;

/// <summary>
/// Representa un intento de integración con un sistema externo.
/// </summary>
public class IntegrationAttempt
{
    public long Id { get; set; }

    /// <summary>
    /// ID del pedido asociado.
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// Referencia de navegación al pedido.
    /// </summary>
    public Order Order { get; set; } = default!;

    /// <summary>
    /// Sistema de destino.
    /// </summary>
    public TargetSystem TargetSystem { get; set; }

    /// <summary>
    /// Estado del intento de integración.
    /// </summary>
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Pending;

    /// <summary>
    /// Payload enviado al sistema destino (JSON).
    /// </summary>
    public string? RequestPayload { get; set; }

    /// <summary>
    /// Respuesta recibida del sistema destino (JSON).
    /// </summary>
    public string? ResponsePayload { get; set; }

    /// <summary>
    /// Número de intentos realizados.
    /// </summary>
    public int Attempts { get; set; } = 1;

    /// <summary>
    /// Fecha y hora del último intento (UTC).
    /// </summary>
    public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mensaje de error (si aplica).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// ID de correlación para rastreo.
    /// </summary>
    public string? CorrelationId { get; set; }
}
