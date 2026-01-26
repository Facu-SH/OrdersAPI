namespace OrderIntegration.Api.Contracts.Audit;

/// <summary>
/// Respuesta con los datos de un evento de auditoría.
/// </summary>
public class AuditEventResponse
{
    public long Id { get; set; }
    public string EntityType { get; set; } = default!;
    public long EntityId { get; set; }
    public string EventType { get; set; } = default!;
    public DateTime TimestampUtc { get; set; }
    public string? UserOrClient { get; set; }
    public string? Data { get; set; }
    public string? CorrelationId { get; set; }
}
