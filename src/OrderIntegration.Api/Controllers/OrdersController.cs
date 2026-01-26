using Microsoft.AspNetCore.Mvc;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Orders;

namespace OrderIntegration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Crea un nuevo pedido.
    /// </summary>
    /// <param name="request">Datos del pedido a crear.</param>
    /// <returns>El pedido creado.</returns>
    /// <response code="201">Pedido creado exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    /// <response code="409">Ya existe un pedido con ese número.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un pedido por su ID.
    /// </summary>
    /// <param name="id">ID del pedido.</param>
    /// <returns>El pedido solicitado.</returns>
    /// <remarks>Este endpoint se implementará en el paso 4d.</remarks>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(long id)
    {
        // Placeholder - se implementará en paso 4d
        return NotFound(new { error = $"Pedido con ID {id} no encontrado." });
    }
}
