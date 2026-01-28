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

    // Railway / Heroku: escuchar en el puerto que asigna la plataforma (evita 502 Bad Gateway)
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNum))
    {
        builder.WebHost.UseUrls($"http://+:{portNum}");
    }

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
    connectionString = connectionString.Trim();

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

// Necesario para WebApplicationFactory en tests
public partial class Program { }
