namespace OrderIntegration.Api.Infrastructure.Integrations;

/// <summary>
/// Simulador del sistema ERP para pruebas.
/// </summary>
public class ErpSimulator
{
    private readonly ILogger<ErpSimulator> _logger;
    private readonly ErpSimulatorSettings _settings;

    public ErpSimulator(ILogger<ErpSimulator> logger, IConfiguration configuration)
    {
        _logger = logger;
        _settings = new ErpSimulatorSettings();
        configuration.GetSection("ErpIntegration").Bind(_settings);
    }

    /// <summary>
    /// Simula el envío de un pedido al ERP.
    /// </summary>
    /// <param name="orderNumber">Número del pedido.</param>
    /// <param name="payload">Payload JSON a enviar.</param>
    /// <returns>Resultado de la simulación.</returns>
    public async Task<ErpSimulationResult> SendOrderAsync(string orderNumber, string payload)
    {
        _logger.LogInformation(
            "Simulando envío de pedido {OrderNumber} al ERP. Modo: {Mode}", 
            orderNumber, _settings.SimulationMode);

        // Simular latencia de red
        var delay = Random.Shared.Next(_settings.MinLatencyMs, _settings.MaxLatencyMs + 1);
        await Task.Delay(delay);

        // Determinar si debe fallar
        var shouldFail = ShouldFail(orderNumber);

        if (shouldFail)
        {
            _logger.LogWarning("Simulación de fallo en envío al ERP para pedido {OrderNumber}", orderNumber);
            return new ErpSimulationResult
            {
                Success = false,
                Message = GetFailureMessage(orderNumber),
                ErpReference = null
            };
        }

        var erpReference = $"ERP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
        
        _logger.LogInformation(
            "Pedido {OrderNumber} enviado exitosamente al ERP. Referencia: {ErpReference}", 
            orderNumber, erpReference);

        return new ErpSimulationResult
        {
            Success = true,
            Message = "Pedido recibido correctamente por el ERP.",
            ErpReference = erpReference
        };
    }

    private bool ShouldFail(string orderNumber)
    {
        // Verificar si el número de pedido está en la lista de fallos forzados
        if (_settings.ForceFailOrderNumbers.Any(pattern => 
            orderNumber.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar si el número de pedido está en la lista de éxitos forzados
        if (_settings.ForceSuccessOrderNumbers.Any(pattern => 
            orderNumber.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Determinar según el modo de simulación
        return _settings.SimulationMode switch
        {
            SimulationMode.AlwaysSucceed => false,
            SimulationMode.AlwaysFail => true,
            SimulationMode.Random => Random.Shared.NextDouble() < _settings.SimulatedFailureRate,
            _ => Random.Shared.NextDouble() < _settings.SimulatedFailureRate
        };
    }

    private string GetFailureMessage(string orderNumber)
    {
        var messages = new[]
        {
            "ERP no disponible temporalmente. Intente nuevamente.",
            "Error de conexión con el servidor ERP.",
            "Timeout al procesar el pedido en el ERP.",
            "El ERP rechazó el pedido por validación interna."
        };

        return messages[Random.Shared.Next(messages.Length)];
    }
}

/// <summary>
/// Configuración del simulador de ERP.
/// </summary>
public class ErpSimulatorSettings
{
    /// <summary>
    /// Modo de simulación.
    /// </summary>
    public SimulationMode SimulationMode { get; set; } = SimulationMode.Random;

    /// <summary>
    /// Tasa de fallo simulado (0.0 a 1.0). Solo aplica en modo Random.
    /// </summary>
    public double SimulatedFailureRate { get; set; } = 0.1;

    /// <summary>
    /// Latencia mínima simulada en milisegundos.
    /// </summary>
    public int MinLatencyMs { get; set; } = 100;

    /// <summary>
    /// Latencia máxima simulada en milisegundos.
    /// </summary>
    public int MaxLatencyMs { get; set; } = 500;

    /// <summary>
    /// Timeout en segundos para la conexión.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Patrones de números de pedido que siempre fallan.
    /// </summary>
    public List<string> ForceFailOrderNumbers { get; set; } = new();

    /// <summary>
    /// Patrones de números de pedido que siempre tienen éxito.
    /// </summary>
    public List<string> ForceSuccessOrderNumbers { get; set; } = new();
}

/// <summary>
/// Modos de simulación del ERP.
/// </summary>
public enum SimulationMode
{
    /// <summary>
    /// Siempre éxito (excepto ForceFailOrderNumbers).
    /// </summary>
    AlwaysSucceed,

    /// <summary>
    /// Siempre fallo (excepto ForceSuccessOrderNumbers).
    /// </summary>
    AlwaysFail,

    /// <summary>
    /// Aleatorio basado en SimulatedFailureRate.
    /// </summary>
    Random
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
