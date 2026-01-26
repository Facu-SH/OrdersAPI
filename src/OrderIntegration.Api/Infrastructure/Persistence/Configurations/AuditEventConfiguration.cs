using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderIntegration.Api.Domain.Entities;

namespace OrderIntegration.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entidad AuditEvent.
/// </summary>
public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.TimestampUtc)
            .IsRequired();

        builder.Property(a => a.UserOrClient)
            .HasMaxLength(100);

        builder.Property(a => a.Data)
            .HasColumnType("text");

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(50);

        // Índices
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        builder.HasIndex(a => a.TimestampUtc);

        builder.HasIndex(a => a.EventType);

        builder.HasIndex(a => a.CorrelationId);
    }
}
