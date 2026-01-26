using OrderIntegration.Api.Contracts.Audit;
using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Application.Interfaces;

/// <summary>
/// Servicio para registrar y consultar eventos de auditoría.
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

    /// <summary>
    /// Obtiene eventos de auditoría con filtros.
    /// </summary>
    Task<List<AuditEventResponse>> GetEventsAsync(AuditQueryParameters parameters);

    /// <summary>
    /// Obtiene los eventos más recientes.
    /// </summary>
    Task<List<AuditEventResponse>> GetRecentEventsAsync(int limit = 100);
}
