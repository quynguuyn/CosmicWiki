using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;

namespace CosmicWiki.IntegrationTests;

public sealed class DeliveryOrderTests
{
    [Fact]
    public void Delivery_person_claims_a_confirmed_order()
    {
        var order = CreatePaidOrder();
        order.TransitionTo(OrderStatus.Confirmed);
        var deliveryPersonId = Guid.NewGuid();

        order.AssignDeliveryPerson(deliveryPersonId, "space-courier");

        Assert.Equal(OrderStatus.Shipping, order.Status);
        Assert.Equal(deliveryPersonId, order.DeliveryPersonId);
        Assert.Equal("space-courier", order.DeliveryPersonName);
    }

    [Fact]
    public void Delivery_person_cannot_claim_an_order_before_admin_confirmation()
    {
        var order = CreatePaidOrder();

        Assert.Throws<InvalidOperationException>(
            () => order.AssignDeliveryPerson(Guid.NewGuid(), "space-courier"));
    }

    [Fact]
    public void Duplicate_payment_events_do_not_change_a_confirmed_order()
    {
        var order = CreatePaidOrder();
        order.TransitionTo(OrderStatus.Confirmed);

        order.MarkPaid();
        order.MarkPaymentFailed();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(
            [OrderStatus.PendingPayment, OrderStatus.Paid, OrderStatus.Confirmed],
            order.StatusHistory.Select(entry => entry.Status));
    }

    [Fact]
    public void Order_records_each_distinct_status_transition_once()
    {
        var order = CreatePaidOrder();

        order.MarkPaid();
        order.TransitionTo(OrderStatus.Confirmed);
        order.AssignDeliveryPerson(Guid.NewGuid(), "space-courier");
        order.TransitionTo(OrderStatus.Delivered);
        order.TransitionTo(OrderStatus.Returned);

        Assert.Equal(
            [
                OrderStatus.PendingPayment,
                OrderStatus.Paid,
                OrderStatus.Confirmed,
                OrderStatus.Shipping,
                OrderStatus.Delivered,
                OrderStatus.Returned
            ],
            order.StatusHistory.Select(entry => entry.Status));
    }

    [Fact]
    public void Order_preserves_a_trimmed_delivery_address_snapshot()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            "customer@example.com",
            "USD",
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"),
            [OrderLine.Create(Guid.NewGuid(), "SKU-1", "Star map", 100, 1)],
            "  123 Observatory Road, Springfield  ");

        Assert.Equal("123 Observatory Road, Springfield", order.DeliveryAddress);
    }

    [Fact]
    public void Only_assigned_delivery_person_can_update_location_and_complete_order()
    {
        var order = CreatePaidOrder();
        var deliveryPersonId = Guid.NewGuid();
        order.TransitionTo(OrderStatus.Confirmed);
        order.AssignDeliveryPerson(deliveryPersonId, "space-courier");

        Assert.Throws<InvalidOperationException>(
            () => order.UpdateDeliveryLocation(Guid.NewGuid(), 10, 20));

        order.UpdateDeliveryLocation(deliveryPersonId, 10.25, 20.5);
        order.MarkDeliveredBy(deliveryPersonId);

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(
            [OrderStatus.PendingPayment, OrderStatus.Paid, OrderStatus.Confirmed, OrderStatus.Shipping, OrderStatus.Delivered],
            order.StatusHistory.Select(entry => entry.Status));
        Assert.Equal(10.25, order.DeliveryLatitude);
        Assert.Equal(20.5, order.DeliveryLongitude);
        Assert.NotNull(order.DeliveryLocationUpdatedAtUtc);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(double.NaN, 0)]
    public void Invalid_delivery_coordinates_are_rejected(double latitude, double longitude)
    {
        var order = CreatePaidOrder();
        var deliveryPersonId = Guid.NewGuid();
        order.TransitionTo(OrderStatus.Confirmed);
        order.AssignDeliveryPerson(deliveryPersonId, "space-courier");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => order.UpdateDeliveryLocation(deliveryPersonId, latitude, longitude));
    }

    private static Order CreatePaidOrder()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            "customer@example.com",
            "VND",
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"),
            [
                OrderLine.Create(Guid.NewGuid(), "SKU-1", "Star map", 100, 1)
            ]);
        order.MarkPaid();
        return order;
    }
}
