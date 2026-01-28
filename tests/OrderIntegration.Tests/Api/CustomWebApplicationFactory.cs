using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderIntegration.Api.Infrastructure.Persistence;

namespace OrderIntegration.Tests.Api;

/// <summary>
/// Factory personalizada para tests de integración usando base de datos en memoria.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Usar un nombre fijo para que todos los tests de la misma clase compartan la BD
    private readonly string _databaseName = "TestDb_" + Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Sobrescribir configuración para deshabilitar migraciones
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSettings:RunMigrations"] = "false",
                ["ApiSettings:ApiKey"] = "dev-api-key-change-in-production"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover el DbContext existente
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remover cualquier otro registro de AppDbContext
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Agregar DbContext con base de datos en memoria (nombre fijo)
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Construir el service provider
            var sp = services.BuildServiceProvider();

            // Crear el scope y asegurar que la base de datos esté creada
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
