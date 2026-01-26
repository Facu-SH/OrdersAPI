using System.Text.Json;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;
using OrderIntegration.Api.Infrastructure.Persistence;

namespace OrderIntegration.Api.Application.Services;

/// <summary>
/// Implementación del servicio de auditoría.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditService(AppDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RecordEventAsync(
        string entityType,
        long entityId,
        EventType eventType,
        object? data = null,
        string? userOrClient = null,
        string? correlationId = null)
    {
        var auditEvent = new AuditEvent
        {
            EntityType = entityType,
            EntityId = entityId,
            EventType = eventType,
            TimestampUtc = DateTime.UtcNow,
            UserOrClient = userOrClient,
            Data = data != null ? JsonSerializer.Serialize(data, JsonOptions) : null,
            CorrelationId = correlationId
        };

        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Evento de auditoría registrado: {EventType} para {EntityType}:{EntityId}",
            eventType, entityType, entityId);
    }

    /// <inheritdoc />
    public Task RecordOrderEventAsync(
        long orderId,
        EventType eventType,
        object? data = null,
        string? correlationId = null)
    {
        return RecordEventAsync("Order", orderId, eventType, data, correlationId: correlationId);
    }
}
