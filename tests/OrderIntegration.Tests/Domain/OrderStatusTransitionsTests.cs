using OrderIntegration.Api.Domain;
using OrderIntegration.Api.Domain.Enums;

namespace OrderIntegration.Tests.Domain;

public class OrderStatusTransitionsTests
{
    #region Valid Transitions

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Prepared)]
    [InlineData(OrderStatus.Created, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Prepared, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Prepared, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Cancelled)]
    public void IsValidTransition_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        // Act
        var result = OrderStatusTransitions.IsValidTransition(from, to);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Invalid Transitions

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Created, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Prepared, OrderStatus.Created)]
    [InlineData(OrderStatus.Prepared, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Created)]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Prepared)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Created)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Prepared)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Created)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Prepared)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Delivered)]
    public void IsValidTransition_InvalidTransitions_ReturnsFalse(OrderStatus from, OrderStatus to)
    {
        // Act
        var result = OrderStatusTransitions.IsValidTransition(from, to);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(OrderStatus.Created)]
    [InlineData(OrderStatus.Prepared)]
    [InlineData(OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void IsValidTransition_SameStatus_ReturnsFalse(OrderStatus status)
    {
        // Act
        var result = OrderStatusTransitions.IsValidTransition(status, status);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetAllowedTransitions

    [Fact]
    public void GetAllowedTransitions_FromCreated_ReturnsPreparedAndCancelled()
    {
        // Act
        var allowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Created);

        // Assert
        Assert.Contains(OrderStatus.Prepared, allowed);
        Assert.Contains(OrderStatus.Cancelled, allowed);
        Assert.Equal(2, allowed.Count);
    }

    [Fact]
    public void GetAllowedTransitions_FromPrepared_ReturnsDispatchedAndCancelled()
    {
        // Act
        var allowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Prepared);

        // Assert
        Assert.Contains(OrderStatus.Dispatched, allowed);
        Assert.Contains(OrderStatus.Cancelled, allowed);
        Assert.Equal(2, allowed.Count);
    }

    [Fact]
    public void GetAllowedTransitions_FromDispatched_ReturnsDeliveredAndCancelled()
    {
        // Act
        var allowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Dispatched);

        // Assert
        Assert.Contains(OrderStatus.Delivered, allowed);
        Assert.Contains(OrderStatus.Cancelled, allowed);
        Assert.Equal(2, allowed.Count);
    }

    [Fact]
    public void GetAllowedTransitions_FromDelivered_ReturnsEmpty()
    {
        // Act
        var allowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Delivered);

        // Assert
        Assert.Empty(allowed);
    }

    [Fact]
    public void GetAllowedTransitions_FromCancelled_ReturnsEmpty()
    {
        // Act
        var allowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Cancelled);

        // Assert
        Assert.Empty(allowed);
    }

    #endregion

    #region ValidateTransition

    [Fact]
    public void ValidateTransition_ValidTransition_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() => 
            OrderStatusTransitions.ValidateTransition(OrderStatus.Created, OrderStatus.Prepared));
        
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateTransition_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            OrderStatusTransitions.ValidateTransition(OrderStatus.Created, OrderStatus.Delivered));

        Assert.Contains("Created", exception.Message);
        Assert.Contains("Delivered", exception.Message);
    }

    [Fact]
    public void ValidateTransition_FromFinalState_ThrowsWithAllowedTransitionsInfo()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            OrderStatusTransitions.ValidateTransition(OrderStatus.Delivered, OrderStatus.Cancelled));

        Assert.Contains("Delivered", exception.Message);
    }

    #endregion

    #region Workflow Scenarios

    [Fact]
    public void FullHappyPath_Created_To_Delivered_AllTransitionsValid()
    {
        // Arrange & Act & Assert
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Created, OrderStatus.Prepared));
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Prepared, OrderStatus.Dispatched));
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Dispatched, OrderStatus.Delivered));
    }

    [Fact]
    public void CancellationPath_CanCancelFromAnyNonFinalState()
    {
        // Act & Assert
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Created, OrderStatus.Cancelled));
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Prepared, OrderStatus.Cancelled));
        Assert.True(OrderStatusTransitions.IsValidTransition(OrderStatus.Dispatched, OrderStatus.Cancelled));
        Assert.False(OrderStatusTransitions.IsValidTransition(OrderStatus.Delivered, OrderStatus.Cancelled));
    }

    [Fact]
    public void FinalStates_CannotTransitionToAnything()
    {
        // Delivered is final
        var deliveredAllowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Delivered);
        Assert.Empty(deliveredAllowed);

        // Cancelled is final
        var cancelledAllowed = OrderStatusTransitions.GetAllowedTransitions(OrderStatus.Cancelled);
        Assert.Empty(cancelledAllowed);
    }

    #endregion
}
