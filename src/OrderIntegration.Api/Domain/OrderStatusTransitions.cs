using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Domain;

/// <summary>
/// Define y valida las transiciones permitidas entre estados de pedido.
/// </summary>
public static class OrderStatusTransitions
{
    /// <summary>
    /// Mapa de transiciones válidas: Estado actual → Estados permitidos.
    /// </summary>
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Created] = [OrderStatus.Prepared, OrderStatus.Cancelled],
        [OrderStatus.Prepared] = [OrderStatus.Dispatched, OrderStatus.Cancelled],
        [OrderStatus.Dispatched] = [OrderStatus.Delivered, OrderStatus.Cancelled],
        [OrderStatus.Delivered] = [], // Estado final, no permite transiciones
        [OrderStatus.Cancelled] = []  // Estado final, no permite transiciones
    };

    /// <summary>
    /// Verifica si una transición de estado es válida.
    /// </summary>
    /// <param name="currentStatus">Estado actual del pedido.</param>
    /// <param name="newStatus">Estado al que se quiere transicionar.</param>
    /// <returns>True si la transición es válida, false en caso contrario.</returns>
    public static bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus == newStatus)
            return false; // No permitir transición al mismo estado

        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses))
            return false;

        return allowedStatuses.Contains(newStatus);
    }

    /// <summary>
    /// Obtiene los estados a los que se puede transicionar desde el estado actual.
    /// </summary>
    /// <param name="currentStatus">Estado actual del pedido.</param>
    /// <returns>Lista de estados permitidos.</returns>
    public static IReadOnlyList<OrderStatus> GetAllowedTransitions(OrderStatus currentStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses)
            ? allowedStatuses
            : [];
    }

    /// <summary>
    /// Valida una transición y lanza excepción si no es válida.
    /// </summary>
    /// <param name="currentStatus">Estado actual del pedido.</param>
    /// <param name="newStatus">Estado al que se quiere transicionar.</param>
    /// <exception cref="InvalidOperationException">Si la transición no es válida.</exception>
    public static void ValidateTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (!IsValidTransition(currentStatus, newStatus))
        {
            var allowed = GetAllowedTransitions(currentStatus);
            var allowedStr = allowed.Any() 
                ? string.Join(", ", allowed) 
                : "ninguno (estado final)";
            
            throw new InvalidOperationException(
                $"Transición de estado no válida: {currentStatus} → {newStatus}. " +
                $"Transiciones permitidas desde {currentStatus}: {allowedStr}.");
        }
    }
}
