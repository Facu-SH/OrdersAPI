namespace OrderIntegration.Api.Middleware;

/// <summary>
/// Middleware para autenticación mediante API Key.
/// </summary>
public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-KEY";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rutas que no requieren autenticación
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        // Verificar si el header API Key está presente
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            _logger.LogWarning("Request sin API Key: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/401",
                title = "No autorizado",
                status = 401,
                detail = "API Key no proporcionada. Incluya el header 'X-API-KEY'.",
                instance = context.Request.Path.Value
            });
            return;
        }

        // Obtener la API Key configurada
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var configuredApiKey = configuration["ApiSettings:ApiKey"];

        if (string.IsNullOrEmpty(configuredApiKey))
        {
            _logger.LogError("API Key no configurada en el servidor");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/500",
                title = "Error de configuración",
                status = 500,
                detail = "Error de configuración del servidor.",
                instance = context.Request.Path.Value
            });
            return;
        }

        // Validar la API Key
        if (!string.Equals(providedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("API Key inválida proporcionada para: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/401",
                title = "No autorizado",
                status = 401,
                detail = "API Key inválida.",
                instance = context.Request.Path.Value
            });
            return;
        }

        await _next(context);
    }

    private static bool IsExcludedPath(string path)
    {
        // Rutas públicas que no requieren API Key
        var excludedPaths = new[]
        {
            "/health",
            "/swagger",
            "/"
        };

        return excludedPaths.Any(excluded => 
            path.Equals(excluded, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(excluded + "/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extensiones para registrar el middleware de API Key.
/// </summary>
public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
