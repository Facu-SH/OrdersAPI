namespace OrderIntegration.Api.Contracts.Integration;

/// <summary>
/// Respuesta del envío al ERP.
/// </summary>
public class SendToErpResponse
{
    public long IntegrationAttemptId { get; set; }
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public bool Success { get; set; }
    public string Status { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? ErpReference { get; set; }
    public DateTime SentAt { get; set; }
}
