using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Application.Services;
using OrderIntegration.Api.Infrastructure.Integrations;
using OrderIntegration.Api.Infrastructure.Persistence;
using OrderIntegration.Api.Middleware;
using Serilog;
using Serilog.Events;

// Configurar Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando Order Integration API");

    var builder = WebApplication.CreateBuilder(args);

    // Railway / Heroku: escuchar en el puerto que asigna la plataforma
    // Si no hay PORT definido, usar 8080 como fallback
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://+:{port}");
    Log.Information("Configurado para escuchar en puerto {Port}", port);

    // Usar Serilog como proveedor de logging
    builder.Host.UseSerilog();

    // Add services to the container.

    // Database: Connection string desde config o DATABASE_URL (Railway inyecta esta variable al vincular Postgres)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "No se encontró connection string. Configure 'ConnectionStrings__DefaultConnection' o asegúrese de que DATABASE_URL está definida (Railway: vincule el servicio Postgres).");
    }
    connectionString = ConvertPostgresUrlToConnectionString(connectionString.Trim());
    Log.Information("Connection string configurada correctamente");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Infrastructure
    builder.Services.AddScoped<ErpSimulator>();

    // Application Services
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IIntegrationService, IntegrationService>();

    // Correlation ID
    builder.Services.AddCorrelationId();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Order Integration API",
            Version = "v1",
            Description = "API REST para gestión de pedidos e integración WMS-ERP.\n\n" +
                          "**Autenticación:** Incluir header `X-API-KEY` en todas las peticiones a /api/*.\n" +
                          "Rutas públicas: /health, /swagger"
        });
    });

    var app = builder.Build();

    // Inicializar base de datos (migraciones + seed en desarrollo)
    var runMigrations = builder.Configuration.GetValue<bool>("ApiSettings:RunMigrations");
    if (runMigrations || app.Environment.IsDevelopment())
    {
        await DbInitializer.InitializeAsync(app.Services, runMigrations: true);
    }

    // Configure the HTTP request pipeline.

    // Correlation ID (primero para que esté disponible en todo el pipeline)
    app.UseCorrelationId();

    // Request logging con Serilog (método, path, status, tiempo)
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            
            if (httpContext.Items.TryGetValue("X-Correlation-Id", out var correlationId))
            {
                diagnosticContext.Set("CorrelationId", correlationId?.ToString() ?? "unknown");
            }
        };
    });

    // Manejo global de excepciones
    app.UseExceptionHandling();

    // Swagger habilitado en todos los entornos (para demo público)
    // En producción real, considerar deshabilitar o proteger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Integration API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz
        options.DocumentTitle = "Order Integration API";
    });

    // Solo redirigir a HTTPS si no estamos en Docker/contenedor
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Autenticación por API Key
    app.UseApiKeyAuthentication();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Convierte una URL de PostgreSQL (formato URI) a connection string de Npgsql.
/// Railway/Heroku usan el formato URI: postgresql://user:pass@host:port/database
/// Npgsql espera: Host=...;Port=...;Database=...;Username=...;Password=...
/// </summary>
static string ConvertPostgresUrlToConnectionString(string url)
{
    // Si ya está en formato connection string (contiene "Host=" o ";"), devolverlo tal cual
    if (url.Contains("Host=", StringComparison.OrdinalIgnoreCase) || 
        url.Contains(";"))
    {
        return url;
    }

    // Intentar parsear como URI
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
        // No es URI válida, devolver tal cual y dejar que Npgsql dé el error específico
        return url;
    }

    // Verificar que es postgres/postgresql
    if (uri.Scheme != "postgres" && uri.Scheme != "postgresql")
    {
        return url;
    }

    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "postgres";
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
}

// Necesario para WebApplicationFactory en tests
public partial class Program { }
