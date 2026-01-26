namespace OrderIntegration.Api.Domain.Enums;

/// <summary>
/// Tipos de eventos de auditoría.
/// </summary>
public enum EventType
{
    /// <summary>
    /// Pedido creado.
    /// </summary>
    OrderCreated,

    /// <summary>
    /// Estado del pedido cambió.
    /// </summary>
    StatusChanged,

    /// <summary>
    /// Pedido enviado al ERP.
    /// </summary>
    ErpSent,

    /// <summary>
    /// ERP confirmó recepción (ACK).
    /// </summary>
    ErpAck,

    /// <summary>
    /// Fallo en integración con ERP.
    /// </summary>
    ErpFail
}
