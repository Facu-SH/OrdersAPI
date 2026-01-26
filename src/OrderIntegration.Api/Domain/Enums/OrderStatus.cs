namespace OrderIntegration.Api.Domain.Enums;

/// <summary>
/// Estados posibles de un pedido en el flujo de trabajo.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Pedido creado, pendiente de preparación.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Pedido preparado, listo para despacho.
    /// </summary>
    Prepared = 1,

    /// <summary>
    /// Pedido despachado, en camino al cliente.
    /// </summary>
    Dispatched = 2,

    /// <summary>
    /// Pedido entregado al cliente.
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Pedido cancelado.
    /// </summary>
    Cancelled = 4
}
