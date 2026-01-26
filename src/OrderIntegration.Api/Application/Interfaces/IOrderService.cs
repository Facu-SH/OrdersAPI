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
}
