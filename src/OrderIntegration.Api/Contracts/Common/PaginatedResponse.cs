namespace OrderIntegration.Api.Contracts.Common;

/// <summary>
/// Respuesta paginada genérica.
/// </summary>
/// <typeparam name="T">Tipo de los elementos.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Página actual (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamaño de página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de elementos.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica si hay página anterior.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si hay página siguiente.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    public PaginatedResponse() { }

    public PaginatedResponse(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
