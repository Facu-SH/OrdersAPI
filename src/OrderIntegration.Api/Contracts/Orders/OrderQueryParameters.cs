using System.ComponentModel.DataAnnotations;

namespace OrderIntegration.Api.Contracts.Orders;

/// <summary>
/// Parámetros de consulta para filtrar y paginar pedidos.
/// </summary>
public class OrderQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Filtrar por estado del pedido.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filtrar por código de cliente.
    /// </summary>
    public string? CustomerCode { get; set; }

    /// <summary>
    /// Filtrar por número de pedido (búsqueda parcial).
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// Fecha de creación desde (inclusive).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Fecha de creación hasta (inclusive).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Número de página (1-based). Por defecto: 1
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página. Por defecto: 10, máximo: 100
    /// </summary>
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
