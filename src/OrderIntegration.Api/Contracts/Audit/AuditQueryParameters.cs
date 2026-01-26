using System.ComponentModel.DataAnnotations;

namespace OrderIntegration.Api.Contracts.Audit;

/// <summary>
/// Parámetros de consulta para filtrar eventos de auditoría.
/// </summary>
public class AuditQueryParameters
{
    /// <summary>
    /// Filtrar por tipo de entidad (ej: "Order").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filtrar por ID de entidad.
    /// </summary>
    public long? EntityId { get; set; }

    /// <summary>
    /// Filtrar por tipo de evento.
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// Filtrar por ID de correlación.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Límite de resultados. Por defecto: 50, máximo: 500
    /// </summary>
    [Range(1, 500, ErrorMessage = "El límite debe estar entre 1 y 500.")]
    public int Limit { get; set; } = 50;
}
