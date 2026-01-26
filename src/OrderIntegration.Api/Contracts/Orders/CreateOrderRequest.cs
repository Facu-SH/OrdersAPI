using System.ComponentModel.DataAnnotations;

namespace OrderIntegration.Api.Contracts.Orders;

/// <summary>
/// Request para crear un nuevo pedido.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Número único del pedido.
    /// </summary>
    [Required(ErrorMessage = "El número de pedido es requerido.")]
    [StringLength(50, ErrorMessage = "El número de pedido no puede exceder 50 caracteres.")]
    public string OrderNumber { get; set; } = default!;

    /// <summary>
    /// Código del cliente.
    /// </summary>
    [Required(ErrorMessage = "El código de cliente es requerido.")]
    [StringLength(50, ErrorMessage = "El código de cliente no puede exceder 50 caracteres.")]
    public string CustomerCode { get; set; } = default!;

    /// <summary>
    /// Ítems del pedido.
    /// </summary>
    [Required(ErrorMessage = "Debe incluir al menos un ítem.")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un ítem.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request para un ítem del pedido.
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// Código SKU del producto.
    /// </summary>
    [Required(ErrorMessage = "El SKU es requerido.")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres.")]
    public string Sku { get; set; } = default!;

    /// <summary>
    /// Descripción del producto.
    /// </summary>
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres.")]
    public string? Description { get; set; }

    /// <summary>
    /// Cantidad solicitada.
    /// </summary>
    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Quantity { get; set; }

    /// <summary>
    /// Precio unitario.
    /// </summary>
    [Required(ErrorMessage = "El precio unitario es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal UnitPrice { get; set; }
}
