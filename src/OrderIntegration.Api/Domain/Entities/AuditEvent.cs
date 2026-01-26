using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Domain.Entities;

/// <summary>
/// Representa un evento de auditoría en el sistema.
/// </summary>
public class AuditEvent
{
    public long Id { get; set; }

    /// <summary>
    /// Tipo de entidad afectada (ej: "Order", "Integration").
    /// </summary>
    public string EntityType { get; set; } = default!;

    /// <summary>
    /// ID de la entidad afectada.
    /// </summary>
    public long EntityId { get; set; }

    /// <summary>
    /// Tipo de evento.
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// Fecha y hora del evento (UTC).
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario o cliente que generó el evento.
    /// </summary>
    public string? UserOrClient { get; set; }

    /// <summary>
    /// Datos adicionales del evento (JSON).
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// ID de correlación para rastreo.
    /// </summary>
    public string? CorrelationId { get; set; }
}
