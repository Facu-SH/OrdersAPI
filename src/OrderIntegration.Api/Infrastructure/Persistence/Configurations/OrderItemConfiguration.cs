using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderIntegration.Api.Domain.Entities;

namespace OrderIntegration.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entidad OrderItem.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Description)
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        // LineTotal es calculado, lo ignoramos en la persistencia
        builder.Ignore(i => i.LineTotal);

        // Índices
        builder.HasIndex(i => i.OrderId);

        builder.HasIndex(i => i.Sku);
    }
}
