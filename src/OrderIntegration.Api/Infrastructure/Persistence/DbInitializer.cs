using Microsoft.EntityFrameworkCore;
using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Infrastructure.Persistence;

/// <summary>
/// Inicializador de base de datos para desarrollo.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Inicializa la base de datos aplicando migraciones y seed data.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool runMigrations = true)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            if (runMigrations)
            {
                logger.LogInformation("Aplicando migraciones de base de datos...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas correctamente.");
            }

            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar la base de datos.");
            throw;
        }
    }

    private static async Task SeedDataAsync(AppDbContext context, ILogger logger)
    {
        // Solo hacer seed si no hay datos
        if (await context.Orders.AnyAsync())
        {
            logger.LogInformation("La base de datos ya contiene datos. Omitiendo seed.");
            return;
        }

        logger.LogInformation("Insertando datos de prueba...");

        var orders = new List<Order>
        {
            new()
            {
                OrderNumber = "ORD-2026-0001",
                CustomerCode = "CUST-001",
                Status = OrderStatus.Created,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                Items = new List<OrderItem>
                {
                    new() { Sku = "SKU-LAPTOP-001", Description = "Laptop HP 15.6\"", Quantity = 2, UnitPrice = 899.99m },
                    new() { Sku = "SKU-MOUSE-001", Description = "Mouse inalámbrico", Quantity = 2, UnitPrice = 29.99m }
                }
            },
            new()
            {
                OrderNumber = "ORD-2026-0002",
                CustomerCode = "CUST-002",
                Status = OrderStatus.Prepared,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem>
                {
                    new() { Sku = "SKU-MONITOR-001", Description = "Monitor 27\" 4K", Quantity = 1, UnitPrice = 449.99m },
                    new() { Sku = "SKU-KEYBOARD-001", Description = "Teclado mecánico", Quantity = 1, UnitPrice = 149.99m },
                    new() { Sku = "SKU-CABLE-HDMI", Description = "Cable HDMI 2m", Quantity = 2, UnitPrice = 15.99m }
                }
            },
            new()
            {
                OrderNumber = "ORD-2026-0003",
                CustomerCode = "CUST-001",
                Status = OrderStatus.Dispatched,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Items = new List<OrderItem>
                {
                    new() { Sku = "SKU-PHONE-001", Description = "Smartphone Android", Quantity = 1, UnitPrice = 699.99m }
                }
            }
        };

        // Calcular totales
        foreach (var order in orders)
        {
            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        logger.LogInformation("Datos de prueba insertados: {Count} pedidos.", orders.Count);
    }
}
