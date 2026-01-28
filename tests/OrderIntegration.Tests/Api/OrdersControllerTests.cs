using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OrderIntegration.Api.Contracts.Orders;

namespace OrderIntegration.Tests.Api;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        // Agregar API Key para autenticación
        _client.DefaultRequestHeaders.Add("X-API-KEY", "dev-api-key-change-in-production");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region POST /api/orders (Create Order)

    [Fact]
    public async Task CreateOrder_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            OrderNumber = "TEST-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Description = "Test Item", Quantity = 2, UnitPrice = 10.50m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        Assert.NotNull(orderResponse);
        Assert.Equal(request.OrderNumber, orderResponse.OrderNumber);
        Assert.Equal(request.CustomerCode, orderResponse.CustomerCode);
        Assert.Equal("Created", orderResponse.Status);
        Assert.Single(orderResponse.Items);
        Assert.Equal(21.00m, orderResponse.TotalAmount); // 2 * 10.50
    }

    [Fact]
    public async Task CreateOrder_MissingOrderNumber_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            OrderNumber = "", // Vacío
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmptyItems_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            OrderNumber = "TEST-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>() // Vacío
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_DuplicateOrderNumber_Returns409Conflict()
    {
        // Arrange
        var orderNumber = "DUP-" + Guid.NewGuid().ToString()[..8];
        var request = new CreateOrderRequest
        {
            OrderNumber = orderNumber,
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        // Crear el primer pedido
        await _client.PostAsJsonAsync("/api/orders", request);

        // Act - Intentar crear otro con el mismo número
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    #endregion

    #region GET /api/orders/{id}

    [Fact]
    public async Task GetOrderById_ExistingOrder_Returns200Ok()
    {
        // Arrange - Crear un pedido primero
        var createRequest = new CreateOrderRequest
        {
            OrderNumber = "GET-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        Assert.NotNull(order);
        Assert.Equal(createdOrder.OrderNumber, order.OrderNumber);
    }

    [Fact]
    public async Task GetOrderById_NonExistingOrder_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/orders/{id}/status (Change Status)

    [Fact]
    public async Task ChangeStatus_ValidTransition_Returns200Ok()
    {
        // Arrange - Crear un pedido
        var createRequest = new CreateOrderRequest
        {
            OrderNumber = "STATUS-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act - Cambiar de Created a Prepared
        var statusRequest = new UpdateStatusRequest { NewStatus = "Prepared" };
        var response = await _client.PostAsJsonAsync($"/api/orders/{createdOrder!.Id}/status", statusRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Prepared", updatedOrder.Status);
    }

    [Fact]
    public async Task ChangeStatus_InvalidTransition_Returns409Conflict()
    {
        // Arrange - Crear un pedido
        var createRequest = new CreateOrderRequest
        {
            OrderNumber = "INVALID-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act - Intentar saltar de Created a Delivered (inválido)
        var statusRequest = new UpdateStatusRequest { NewStatus = "Delivered" };
        var response = await _client.PostAsJsonAsync($"/api/orders/{createdOrder!.Id}/status", statusRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_NonExistingOrder_Returns404NotFound()
    {
        // Act
        var statusRequest = new UpdateStatusRequest { NewStatus = "Prepared" };
        var response = await _client.PostAsJsonAsync("/api/orders/99999/status", statusRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_FullWorkflow_SucceedsForHappyPath()
    {
        // Arrange - Crear un pedido
        var createRequest = new CreateOrderRequest
        {
            OrderNumber = "WORKFLOW-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act & Assert - Created -> Prepared
        var response1 = await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/status", 
            new UpdateStatusRequest { NewStatus = "Prepared" });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Prepared -> Dispatched
        var response2 = await _client.PostAsJsonAsync($"/api/orders/{order.Id}/status", 
            new UpdateStatusRequest { NewStatus = "Dispatched" });
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        // Dispatched -> Delivered
        var response3 = await _client.PostAsJsonAsync($"/api/orders/{order.Id}/status", 
            new UpdateStatusRequest { NewStatus = "Delivered" });
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        // Verificar estado final
        var finalResponse = await _client.GetAsync($"/api/orders/{order.Id}");
        var finalOrder = await finalResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        Assert.Equal("Delivered", finalOrder!.Status);
    }

    [Fact]
    public async Task ChangeStatus_ToCancelled_SucceedsFromNonFinalState()
    {
        // Arrange - Crear un pedido
        var createRequest = new CreateOrderRequest
        {
            OrderNumber = "CANCEL-" + Guid.NewGuid().ToString()[..8],
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act - Cancelar desde Created
        var response = await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/status", 
            new UpdateStatusRequest { NewStatus = "Cancelled" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cancelledOrder = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        Assert.Equal("Cancelled", cancelledOrder!.Status);
    }

    #endregion

    #region GET /api/orders (List with filters)

    [Fact]
    public async Task GetOrders_ReturnsListWithPagination()
    {
        // Arrange - Crear algunos pedidos
        for (int i = 0; i < 3; i++)
        {
            var request = new CreateOrderRequest
            {
                OrderNumber = $"LIST-{Guid.NewGuid().ToString()[..8]}",
                CustomerCode = "CUST-LIST",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
                }
            };
            await _client.PostAsJsonAsync("/api/orders", request);
        }

        // Act
        var response = await _client.GetAsync("/api/orders?pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Authentication

    [Fact]
    public async Task CreateOrder_WithoutApiKey_Returns401Unauthorized()
    {
        // Arrange
        var client = new CustomWebApplicationFactory().CreateClient();
        // NO agregar API Key

        var request = new CreateOrderRequest
        {
            OrderNumber = "AUTH-TEST",
            CustomerCode = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
