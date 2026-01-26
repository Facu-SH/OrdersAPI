namespace OrderIntegration.Api.Middleware;

/// <summary>
/// Middleware para manejar el ID de correlación en requests.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener o generar el CorrelationId
        var correlationId = GetOrGenerateCorrelationId(context);

        // Guardar en HttpContext.Items para uso interno
        context.Items[CorrelationIdHeader] = correlationId;

        // Agregar al TraceIdentifier para logs
        context.TraceIdentifier = correlationId;

        // Agregar header de respuesta
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });

        // Agregar al scope de logging
        using (context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Si viene en el header, usarlo
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generar uno nuevo
        return Guid.NewGuid().ToString("N")[..12]; // 12 caracteres para ser más corto
    }
}

/// <summary>
/// Servicio para acceder al CorrelationId actual.
/// </summary>
public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}

/// <summary>
/// Implementación del accessor de CorrelationId.
/// </summary>
public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Items[CorrelationIdHeader]?.ToString();
}

/// <summary>
/// Extensiones para registrar el middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }
}
