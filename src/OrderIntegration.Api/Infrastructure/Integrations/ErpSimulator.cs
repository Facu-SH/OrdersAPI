namespace OrderIntegration.Api.Infrastructure.Integrations;

/// <summary>
/// Simulador del sistema ERP para pruebas.
/// </summary>
public class ErpSimulator
{
    private readonly ILogger<ErpSimulator> _logger;
    private readonly double _failureRate;

    public ErpSimulator(ILogger<ErpSimulator> logger, IConfiguration configuration)
    {
        _logger = logger;
        _failureRate = configuration.GetValue<double>("ErpIntegration:SimulatedFailureRate", 0.1);
    }

    /// <summary>
    /// Simula el envío de un pedido al ERP.
    /// </summary>
    /// <param name="orderNumber">Número del pedido.</param>
    /// <param name="payload">Payload JSON a enviar.</param>
    /// <returns>Resultado de la simulación.</returns>
    public async Task<ErpSimulationResult> SendOrderAsync(string orderNumber, string payload)
    {
        _logger.LogInformation("Simulando envío de pedido {OrderNumber} al ERP", orderNumber);

        // Simular latencia de red
        await Task.Delay(Random.Shared.Next(100, 500));

        // Determinar si falla basado en la tasa de fallo configurada
        var shouldFail = Random.Shared.NextDouble() < _failureRate;

        if (shouldFail)
        {
            _logger.LogWarning("Simulación de fallo en envío al ERP para pedido {OrderNumber}", orderNumber);
            return new ErpSimulationResult
            {
                Success = false,
                Message = "ERP no disponible temporalmente. Intente nuevamente.",
                ErpReference = null
            };
        }

        var erpReference = $"ERP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
        
        _logger.LogInformation("Pedido {OrderNumber} enviado exitosamente al ERP. Referencia: {ErpReference}", 
            orderNumber, erpReference);

        return new ErpSimulationResult
        {
            Success = true,
            Message = "Pedido recibido correctamente por el ERP.",
            ErpReference = erpReference
        };
    }
}

/// <summary>
/// Resultado de la simulación de envío al ERP.
/// </summary>
public class ErpSimulationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? ErpReference { get; set; }
}
