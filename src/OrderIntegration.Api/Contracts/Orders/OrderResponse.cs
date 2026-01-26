namespace OrderIntegration.Api.Contracts.Orders;

/// <summary>
/// Respuesta con los datos de un pedido.
/// </summary>
public class OrderResponse
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string CustomerCode { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();

    /// <summary>
    /// Estados a los que puede transicionar el pedido.
    /// </summary>
    public List<string> AllowedTransitions { get; set; } = new();
}

/// <summary>
/// Respuesta con los datos de un ítem del pedido.
/// </summary>
public class OrderItemResponse
{
    public long Id { get; set; }
    public string Sku { get; set; } = default!;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
