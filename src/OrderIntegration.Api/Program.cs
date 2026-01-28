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

    // Usar Serilog como proveedor de logging
    builder.Host.UseSerilog();

    // Add services to the container.

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Integration API v1");
            options.RoutePrefix = string.Empty; // Swagger en la raíz
        });
    }

    app.UseHttpsRedirection();

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
