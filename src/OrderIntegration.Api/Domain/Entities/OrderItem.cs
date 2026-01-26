namespace OrderIntegration.Api.Domain.Entities;

/// <summary>
/// Representa un ítem dentro de un pedido.
/// </summary>
public class OrderItem
{
    public long Id { get; set; }

    /// <summary>
    /// ID del pedido al que pertenece este ítem.
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// Referencia de navegación al pedido.
    /// </summary>
    public Order Order { get; set; } = default!;

    /// <summary>
    /// Código SKU del producto.
    /// </summary>
    public string Sku { get; set; } = default!;

    /// <summary>
    /// Descripción del producto.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Cantidad solicitada.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total de la línea (Quantity * UnitPrice).
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;
}
