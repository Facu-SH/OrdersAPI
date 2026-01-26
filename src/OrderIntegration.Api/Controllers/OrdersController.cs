using Microsoft.AspNetCore.Mvc;
using OrderIntegration.Api.Application.Interfaces;
using OrderIntegration.Api.Contracts.Common;
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
    /// Obtiene pedidos con filtros y paginación.
    /// </summary>
    /// <param name="parameters">Parámetros de filtro y paginación.</param>
    /// <returns>Lista paginada de pedidos.</returns>
    /// <response code="200">Lista de pedidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<OrderResponse>>> GetOrders([FromQuery] OrderQueryParameters parameters)
    {
        var result = await _orderService.GetOrdersAsync(parameters);
        return Ok(result);
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
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(long id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            return NotFound(new { error = $"Pedido con ID {id} no encontrado." });
        }

        return Ok(order);
    }

    /// <summary>
    /// Actualiza el estado de un pedido.
    /// </summary>
    /// <param name="id">ID del pedido.</param>
    /// <param name="request">Nuevo estado del pedido.</param>
    /// <returns>El pedido actualizado.</returns>
    /// <response code="200">Estado actualizado exitosamente.</response>
    /// <response code="404">Pedido no encontrado.</response>
    /// <response code="409">Transición de estado no válida.</response>
    [HttpPost("{id:long}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(id, request);
            
            if (order == null)
            {
                return NotFound(new { error = $"Pedido con ID {id} no encontrado." });
            }

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
