using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Audit;
using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;
using OrderIntegration.Api.Infrastructure.Persistence;
using OrderIntegration.Api.Middleware;

namespace OrderIntegration.Api.Application.Services;

/// <summary>
/// Implementación del servicio de auditoría.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditService(
        AppDbContext context,
        ILogger<AuditService> logger,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _context = context;
        _logger = logger;
        _correlationIdAccessor = correlationIdAccessor;
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
        // Usar el correlationId del contexto si no se proporciona uno explícitamente
        var effectiveCorrelationId = correlationId ?? _correlationIdAccessor.CorrelationId;

        var auditEvent = new AuditEvent
        {
            EntityType = entityType,
            EntityId = entityId,
            EventType = eventType,
            TimestampUtc = DateTime.UtcNow,
            UserOrClient = userOrClient,
            Data = data != null ? JsonSerializer.Serialize(data, JsonOptions) : null,
            CorrelationId = effectiveCorrelationId
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

    /// <inheritdoc />
    public async Task<List<AuditEventResponse>> GetEventsAsync(AuditQueryParameters parameters)
    {
        var query = _context.AuditEvents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.EntityType))
        {
            query = query.Where(e => e.EntityType == parameters.EntityType);
        }

        if (parameters.EntityId.HasValue)
        {
            query = query.Where(e => e.EntityId == parameters.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.EventType))
        {
            if (Enum.TryParse<EventType>(parameters.EventType, ignoreCase: true, out var eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.CorrelationId))
        {
            query = query.Where(e => e.CorrelationId == parameters.CorrelationId);
        }

        var events = await query
            .OrderByDescending(e => e.TimestampUtc)
            .Take(parameters.Limit)
            .ToListAsync();

        return events.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<List<AuditEventResponse>> GetRecentEventsAsync(int limit = 100)
    {
        // Validar límite
        limit = Math.Clamp(limit, 1, 500);

        var events = await _context.AuditEvents
            .OrderByDescending(e => e.TimestampUtc)
            .Take(limit)
            .ToListAsync();

        return events.Select(MapToResponse).ToList();
    }

    private static AuditEventResponse MapToResponse(AuditEvent auditEvent)
    {
        return new AuditEventResponse
        {
            Id = auditEvent.Id,
            EntityType = auditEvent.EntityType,
            EntityId = auditEvent.EntityId,
            EventType = auditEvent.EventType.ToString(),
            TimestampUtc = auditEvent.TimestampUtc,
            UserOrClient = auditEvent.UserOrClient,
            Data = auditEvent.Data,
            CorrelationId = auditEvent.CorrelationId
        };
    }
}
