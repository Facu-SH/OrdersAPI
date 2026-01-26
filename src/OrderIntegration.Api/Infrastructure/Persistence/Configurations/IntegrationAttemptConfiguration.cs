using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderIntegration.Api.Domain.Entities;

namespace OrderIntegration.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entidad IntegrationAttempt.
/// </summary>
public class IntegrationAttemptConfiguration : IEntityTypeConfiguration<IntegrationAttempt>
{
    public void Configure(EntityTypeBuilder<IntegrationAttempt> builder)
    {
        builder.ToTable("IntegrationAttempts");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.OrderId)
            .IsRequired();

        builder.Property(i => i.TargetSystem)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.RequestPayload)
            .HasColumnType("text");

        builder.Property(i => i.ResponsePayload)
            .HasColumnType("text");

        builder.Property(i => i.Attempts)
            .IsRequired();

        builder.Property(i => i.LastAttemptAt)
            .IsRequired();

        builder.Property(i => i.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(i => i.CorrelationId)
            .HasMaxLength(50);

        // Índices
        builder.HasIndex(i => i.OrderId);

        builder.HasIndex(i => i.Status);

        builder.HasIndex(i => i.LastAttemptAt);

        builder.HasIndex(i => i.CorrelationId);

        // Relación con Order
        builder.HasOne(i => i.Order)
            .WithMany()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
