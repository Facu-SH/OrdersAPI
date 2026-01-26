using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Integration;
using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;
using OrderIntegration.Api.Infrastructure.Integrations;
using OrderIntegration.Api.Infrastructure.Persistence;

namespace OrderIntegration.Api.Application.Services;

/// <summary>
/// Implementación del servicio de integración.
/// </summary>
public class IntegrationService : IIntegrationService
{
    private readonly AppDbContext _context;
    private readonly ErpSimulator _erpSimulator;
    private readonly IAuditService _auditService;
    private readonly ILogger<IntegrationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public IntegrationService(
        AppDbContext context,
        ErpSimulator erpSimulator,
        IAuditService auditService,
        ILogger<IntegrationService> logger)
    {
        _context = context;
        _erpSimulator = erpSimulator;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SendToErpResponse?> SendOrderToErpAsync(long orderId, string? correlationId = null)
    {
        _logger.LogInformation("Enviando pedido {OrderId} al ERP. CorrelationId: {CorrelationId}", 
            orderId, correlationId);

        // Obtener el pedido
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} no encontrado para enviar al ERP", orderId);
            return null;
        }

        // Preparar el payload
        var payload = new
        {
            order.OrderNumber,
            order.CustomerCode,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            Items = order.Items.Select(i => new
            {
                i.Sku,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            })
        };

        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

        // Crear el intento de integración
        var attempt = new IntegrationAttempt
        {
            OrderId = orderId,
            TargetSystem = TargetSystem.ERP,
            Status = IntegrationStatus.Sent,
            RequestPayload = payloadJson,
            Attempts = 1,
            LastAttemptAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        _context.IntegrationAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Simular envío al ERP
        var result = await _erpSimulator.SendOrderAsync(order.OrderNumber, payloadJson);

        // Actualizar el intento con el resultado
        if (result.Success)
        {
            attempt.Status = IntegrationStatus.Acked;
            attempt.ResponsePayload = JsonSerializer.Serialize(new
            {
                result.Success,
                result.Message,
                result.ErpReference,
                AckedAt = DateTime.UtcNow
            }, JsonOptions);

            // Registrar evento de auditoría
            await _auditService.RecordOrderEventAsync(orderId, EventType.ErpAck, new
            {
                result.ErpReference,
                result.Message,
                attempt.Id
            }, correlationId);
        }
        else
        {
            attempt.Status = IntegrationStatus.Failed;
            attempt.ErrorMessage = result.Message;
            attempt.ResponsePayload = JsonSerializer.Serialize(new
            {
                result.Success,
                result.Message,
                FailedAt = DateTime.UtcNow
            }, JsonOptions);

            // Registrar evento de auditoría
            await _auditService.RecordOrderEventAsync(orderId, EventType.ErpFail, new
            {
                result.Message,
                attempt.Id
            }, correlationId);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Envío al ERP completado para pedido {OrderId}. Éxito: {Success}, IntegrationAttemptId: {AttemptId}", 
            orderId, result.Success, attempt.Id);

        return new SendToErpResponse
        {
            IntegrationAttemptId = attempt.Id,
            OrderId = orderId,
            OrderNumber = order.OrderNumber,
            Success = result.Success,
            Status = attempt.Status.ToString(),
            Message = result.Message,
            ErpReference = result.ErpReference,
            SentAt = attempt.LastAttemptAt
        };
    }

    /// <inheritdoc />
    public async Task<ErpWebhookResponse> ProcessErpWebhookAsync(ErpWebhookRequest request)
    {
        _logger.LogInformation(
            "Procesando webhook del ERP para pedido {OrderNumber}. Success: {Success}, CorrelationId: {CorrelationId}",
            request.OrderNumber, request.Success, request.CorrelationId);

        // Buscar el pedido por número
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber);

        if (order == null)
        {
            _logger.LogWarning("Webhook recibido para pedido inexistente: {OrderNumber}", request.OrderNumber);
            return new ErpWebhookResponse
            {
                Processed = false,
                Message = $"Pedido con número '{request.OrderNumber}' no encontrado.",
                OrderNumber = request.OrderNumber
            };
        }

        // Buscar el último intento de integración pendiente o enviado
        var attempt = await _context.IntegrationAttempts
            .Where(i => i.OrderId == order.Id && 
                        i.TargetSystem == TargetSystem.ERP &&
                        (i.Status == IntegrationStatus.Sent || i.Status == IntegrationStatus.Pending))
            .OrderByDescending(i => i.LastAttemptAt)
            .FirstOrDefaultAsync();

        if (attempt == null)
        {
            _logger.LogWarning(
                "Webhook recibido pero no hay intento de integración pendiente para pedido {OrderNumber}", 
                request.OrderNumber);
            return new ErpWebhookResponse
            {
                Processed = false,
                Message = $"No hay intento de integración pendiente para el pedido '{request.OrderNumber}'.",
                OrderNumber = request.OrderNumber
            };
        }

        // Actualizar el intento
        attempt.LastAttemptAt = DateTime.UtcNow;
        attempt.CorrelationId = request.CorrelationId ?? attempt.CorrelationId;

        if (request.Success)
        {
            attempt.Status = IntegrationStatus.Acked;
            attempt.ResponsePayload = JsonSerializer.Serialize(new
            {
                request.Success,
                request.Message,
                request.ErpReference,
                AckedAt = DateTime.UtcNow,
                Source = "Webhook"
            }, JsonOptions);

            await _auditService.RecordOrderEventAsync(order.Id, EventType.ErpAck, new
            {
                request.ErpReference,
                request.Message,
                attempt.Id,
                Source = "Webhook"
            }, request.CorrelationId);

            _logger.LogInformation(
                "Webhook procesado: Pedido {OrderNumber} confirmado por ERP. Referencia: {ErpReference}",
                request.OrderNumber, request.ErpReference);
        }
        else
        {
            attempt.Status = IntegrationStatus.Failed;
            attempt.ErrorMessage = request.Message;
            attempt.ResponsePayload = JsonSerializer.Serialize(new
            {
                request.Success,
                request.Message,
                FailedAt = DateTime.UtcNow,
                Source = "Webhook"
            }, JsonOptions);

            await _auditService.RecordOrderEventAsync(order.Id, EventType.ErpFail, new
            {
                request.Message,
                attempt.Id,
                Source = "Webhook"
            }, request.CorrelationId);

            _logger.LogWarning(
                "Webhook procesado: Pedido {OrderNumber} rechazado por ERP. Mensaje: {Message}",
                request.OrderNumber, request.Message);
        }

        await _context.SaveChangesAsync();

        return new ErpWebhookResponse
        {
            Processed = true,
            Message = request.Success 
                ? "Confirmación del ERP procesada correctamente." 
                : "Rechazo del ERP registrado.",
            IntegrationAttemptId = attempt.Id,
            OrderNumber = request.OrderNumber
        };
    }
}
