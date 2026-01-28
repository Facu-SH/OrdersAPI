using OrderIntegration.Api.Domain.Entities;
using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Tests.Domain;

public class OrderTests
{
    #region ChangeStatus

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Created);
        var originalUpdatedAt = order.UpdatedAt;

        // Act
        order.ChangeStatus(OrderStatus.Prepared);

        // Assert
        Assert.Equal(OrderStatus.Prepared, order.Status);
        Assert.True(order.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Created);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ChangeStatus(OrderStatus.Delivered));
    }

    [Fact]
    public void ChangeStatus_FromDelivered_ThrowsException()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Delivered);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ChangeStatus(OrderStatus.Cancelled));
    }

    [Fact]
    public void ChangeStatus_ToCancelled_FromCreated_Succeeds()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Created);

        // Act
        order.ChangeStatus(OrderStatus.Cancelled);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    #endregion

    #region CanTransitionTo

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Prepared, true)]
    [InlineData(OrderStatus.Created, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Created, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Prepared, OrderStatus.Dispatched, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled, false)]
    public void CanTransitionTo_ReturnsExpectedResult(OrderStatus current, OrderStatus target, bool expected)
    {
        // Arrange
        var order = CreateTestOrder(current);

        // Act
        var result = order.CanTransitionTo(target);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region CalculateTotalAmount

    [Fact]
    public void CalculateTotalAmount_WithItems_CalculatesCorrectTotal()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "TEST-001",
            CustomerCode = "CUST-001",
            Items = new List<OrderItem>
            {
                new() { Sku = "SKU-1", Quantity = 2, UnitPrice = 10.00m },
                new() { Sku = "SKU-2", Quantity = 3, UnitPrice = 15.50m }
            }
        };

        // Act
        order.CalculateTotalAmount();

        // Assert
        // (2 * 10.00) + (3 * 15.50) = 20.00 + 46.50 = 66.50
        Assert.Equal(66.50m, order.TotalAmount);
    }

    [Fact]
    public void CalculateTotalAmount_EmptyItems_ReturnsZero()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "TEST-001",
            CustomerCode = "CUST-001",
            Items = new List<OrderItem>()
        };

        // Act
        order.CalculateTotalAmount();

        // Assert
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void CalculateTotalAmount_NullItems_ReturnsZero()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "TEST-001",
            CustomerCode = "CUST-001",
            Items = null!
        };

        // Act
        order.CalculateTotalAmount();

        // Assert
        Assert.Equal(0m, order.TotalAmount);
    }

    #endregion

    #region OrderItem LineTotal

    [Fact]
    public void OrderItem_LineTotal_CalculatesCorrectly()
    {
        // Arrange
        var item = new OrderItem
        {
            Sku = "SKU-001",
            Quantity = 5,
            UnitPrice = 12.50m
        };

        // Act
        var lineTotal = item.LineTotal;

        // Assert
        Assert.Equal(62.50m, lineTotal);
    }

    #endregion

    #region Helper Methods

    private static Order CreateTestOrder(OrderStatus status)
    {
        return new Order
        {
            Id = 1,
            OrderNumber = "TEST-001",
            CustomerCode = "CUST-001",
            Status = status,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
            Items = new List<OrderItem>
            {
                new() { Id = 1, Sku = "SKU-001", Quantity = 1, UnitPrice = 10.00m }
            }
        };
    }

    #endregion
}
