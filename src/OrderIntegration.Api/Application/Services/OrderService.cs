using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
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
