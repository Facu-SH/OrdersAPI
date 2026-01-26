using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Common;
using OrderIntegration.Api.Contracts.Orders;
using OrderIntegration.Api.Domain;
using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;
using OrderIntegration.Api.Infrastructure.Persistence;

namespace OrderIntegration.Api.Application.Services;

/// <summary>
/// Implementación del servicio de pedidos.
/// </summary>
public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creando pedido {OrderNumber} para cliente {CustomerCode}", 
            request.OrderNumber, request.CustomerCode);

        // Verificar que no exista un pedido con el mismo número
        var exists = await _context.Orders
            .AnyAsync(o => o.OrderNumber == request.OrderNumber);

        if (exists)
        {
            throw new InvalidOperationException($"Ya existe un pedido con el número '{request.OrderNumber}'.");
        }

        // Crear la entidad
        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            CustomerCode = request.CustomerCode,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                Sku = i.Sku,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        // Calcular total
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido {OrderNumber} creado con ID {OrderId}", 
            order.OrderNumber, order.Id);

        return MapToResponse(order);
    }

    /// <inheritdoc />
    public async Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(OrderQueryParameters parameters)
    {
        _logger.LogInformation("Consultando pedidos con filtros: Status={Status}, Customer={Customer}, Page={Page}", 
            parameters.Status, parameters.CustomerCode, parameters.Page);

        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            if (Enum.TryParse<OrderStatus>(parameters.Status, ignoreCase: true, out var status))
            {
                query = query.Where(o => o.Status == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.CustomerCode))
        {
            query = query.Where(o => o.CustomerCode == parameters.CustomerCode);
        }

        if (!string.IsNullOrWhiteSpace(parameters.OrderNumber))
        {
            query = query.Where(o => o.OrderNumber.Contains(parameters.OrderNumber));
        }

        if (parameters.FromDate.HasValue)
        {
            var fromDate = parameters.FromDate.Value.ToUniversalTime();
            query = query.Where(o => o.CreatedAt >= fromDate);
        }

        if (parameters.ToDate.HasValue)
        {
            var toDate = parameters.ToDate.Value.ToUniversalTime().AddDays(1);
            query = query.Where(o => o.CreatedAt < toDate);
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Aplicar ordenamiento y paginación
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var items = orders.Select(MapToResponse).ToList();

        return new PaginatedResponse<OrderResponse>(items, parameters.Page, parameters.PageSize, totalCount);
    }

    /// <inheritdoc />
    public async Task<OrderResponse?> GetOrderByIdAsync(long id)
    {
        _logger.LogInformation("Obteniendo pedido con ID {OrderId}", id);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Pedido con ID {OrderId} no encontrado", id);
            return null;
        }

        return MapToResponse(order);
    }

    /// <inheritdoc />
    public async Task<OrderResponse?> UpdateStatusAsync(long id, UpdateStatusRequest request)
    {
        _logger.LogInformation("Actualizando estado del pedido {OrderId} a {NewStatus}", id, request.NewStatus);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Pedido con ID {OrderId} no encontrado para actualizar estado", id);
            return null;
        }

        // Parsear el nuevo estado
        if (!Enum.TryParse<OrderStatus>(request.NewStatus, ignoreCase: true, out var newStatus))
        {
            throw new InvalidOperationException(
                $"Estado '{request.NewStatus}' no es válido. Estados válidos: {string.Join(", ", Enum.GetNames<OrderStatus>())}");
        }

        var previousStatus = order.Status;

        // Cambiar estado (incluye validación de transición)
        order.ChangeStatus(newStatus);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido {OrderId} cambió de {PreviousStatus} a {NewStatus}", 
            id, previousStatus, newStatus);

        return MapToResponse(order);
    }

    /// <summary>
    /// Mapea una entidad Order a OrderResponse.
    /// </summary>
    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerCode = order.CustomerCode,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                Sku = i.Sku,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            }).ToList(),
            AllowedTransitions = OrderStatusTransitions
                .GetAllowedTransitions(order.Status)
                .Select(s => s.ToString())
                .ToList()
        };
    }
}
