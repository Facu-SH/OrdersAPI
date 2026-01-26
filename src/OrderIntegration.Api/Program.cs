using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Application.Services;
using OrderIntegration.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Order Integration API",
        Version = "v1",
        Description = "API REST para gestión de pedidos e integración WMS-ERP"
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
app.UseAuthorization();
app.MapControllers();

app.Run();
