using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Application.Interfaces;

/// <summary>
/// Servicio para registrar eventos de auditoría.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra un evento de auditoría.
    /// </summary>
    Task RecordEventAsync(
        string entityType,
        long entityId,
        EventType eventType,
        object? data = null,
        string? userOrClient = null,
        string? correlationId = null);

    /// <summary>
    /// Registra un evento de pedido.
    /// </summary>
    Task RecordOrderEventAsync(
        long orderId,
        EventType eventType,
        object? data = null,
        string? correlationId = null);
}
