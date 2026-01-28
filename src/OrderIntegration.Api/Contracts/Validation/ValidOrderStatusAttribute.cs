using System.ComponentModel.DataAnnotations;
using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Api.Contracts.Validation;

/// <summary>
/// Valida que el valor sea un estado de pedido válido.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidOrderStatusAttribute : ValidationAttribute
{
    private static readonly string[] ValidStatuses = Enum.GetNames<OrderStatus>();

    public ValidOrderStatusAttribute()
    {
        ErrorMessage = $"Estado inválido. Valores permitidos: {string.Join(", ", ValidStatuses)}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // [Required] maneja el caso nulo
        }

        var stringValue = value.ToString();

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return new ValidationResult(ErrorMessage);
        }

        if (Enum.TryParse<OrderStatus>(stringValue, ignoreCase: true, out _))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}
