using System.ComponentModel.DataAnnotations;
using OrderIntegration.Api.Contracts.Validation;

namespace OrderIntegration.Api.Contracts.Orders;

/// <summary>
/// Request para cambiar el estado de un pedido.
/// </summary>
public class UpdateStatusRequest
{
    /// <summary>
    /// Nuevo estado del pedido.
    /// Valores válidos: Created, Prepared, Dispatched, Delivered, Cancelled
    /// </summary>
    [Required(ErrorMessage = "El nuevo estado es requerido.")]
    [ValidOrderStatus]
    public string NewStatus { get; set; } = default!;
}
