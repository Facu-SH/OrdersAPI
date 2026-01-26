using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Domain.Entities;

/// <summary>
/// Representa un pedido en el sistema.
/// </summary>
public class Order
{
    public long Id { get; set; }

    /// <summary>
    /// Número único del pedido (ej: ORD-2026-0001).
    /// </summary>
    public string OrderNumber { get; set; } = default!;

    /// <summary>
    /// Código del cliente que realizó el pedido.
    /// </summary>
    public string CustomerCode { get; set; } = default!;

    /// <summary>
    /// Estado actual del pedido.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Created;

    /// <summary>
    /// Fecha y hora de creación (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de última actualización (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Monto total del pedido.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Ítems del pedido.
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// Calcula el monto total basado en los ítems.
    /// </summary>
    public void CalculateTotalAmount()
    {
        TotalAmount = Items.Sum(item => item.LineTotal);
    }
}
