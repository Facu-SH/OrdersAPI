using OrderIntegration.Api.Contracts.Common;
using OrderIntegration.Api.Contracts.Orders;

namespace OrderIntegration.Api.Application.Interfaces;

/// <summary>
/// Servicio para gestión de pedidos.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Crea un nuevo pedido.
    /// </summary>
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);

    /// <summary>
    /// Obtiene pedidos con filtros y paginación.
    /// </summary>
    Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(OrderQueryParameters parameters);

    /// <summary>
    /// Obtiene un pedido por su ID.
    /// </summary>
    Task<OrderResponse?> GetOrderByIdAsync(long id);

    /// <summary>
    /// Actualiza el estado de un pedido.
    /// </summary>
    /// <returns>El pedido actualizado o null si no existe.</returns>
    /// <exception cref="InvalidOperationException">Si la transición no es válida.</exception>
    Task<OrderResponse?> UpdateStatusAsync(long id, UpdateStatusRequest request);
}
