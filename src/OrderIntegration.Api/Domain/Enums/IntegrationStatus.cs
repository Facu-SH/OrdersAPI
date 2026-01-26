namespace OrderIntegration.Api.Domain.Enums;

/// <summary>
/// Estados de un intento de integración.
/// </summary>
public enum IntegrationStatus
{
    /// <summary>
    /// Pendiente de envío.
    /// </summary>
    Pending,

    /// <summary>
    /// Enviado, esperando respuesta.
    /// </summary>
    Sent,

    /// <summary>
    /// Confirmado por el sistema destino (ACK).
    /// </summary>
    Acked,

    /// <summary>
    /// Falló el envío o fue rechazado.
    /// </summary>
    Failed
}
