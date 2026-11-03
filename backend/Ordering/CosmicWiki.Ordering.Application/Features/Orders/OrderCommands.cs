using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Orders;

public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status)
    : IRequest<Order?>;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orders,
    IInventoryReservationService inventory,
    IOrderEmailSender emailSender,
    IOrderNotificationPublisher notificationPublisher)
    : IRequestHandler<UpdateOrderStatusCommand, Order?>
{
    public async Task<Order?> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
            return null;

        if (!order.TransitionTo(request.Status))
        {
            if (request.Status == OrderStatus.Cancelled)
                await inventory.ReleaseAsync(order.Id, cancellationToken);
            return order;
        }

        await orders.SaveChangesAsync(cancellationToken);
        await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        if (request.Status == OrderStatus.Cancelled)
            await inventory.ReleaseAsync(order.Id, cancellationToken);
        await SendStatusEmailAsync(order, cancellationToken);
        return order;
    }

    internal static Task SendStatusEmailAsync(Order order, IOrderEmailSender emailSender, CancellationToken cancellationToken) =>
        emailSender.SendOrderStatusChangedAsync(
            order.Id,
            order.CustomerEmail,
            order.TotalAmount,
            order.Currency,
            order.Status.ToString(),
            ToEmailLines(order),
            cancellationToken);

    private Task SendStatusEmailAsync(Order order, CancellationToken cancellationToken) =>
        SendStatusEmailAsync(order, emailSender, cancellationToken);

    internal static IReadOnlyCollection<OrderCreatedLine> ToEmailLines(Order order) =>
        order.Lines.Select(line => new OrderCreatedLine(
            line.SKU,
            line.ProductName,
            line.UnitPrice,
            line.Quantity,
            line.LineTotal)).ToArray();
}

public sealed record ClaimDeliveryOrderCommand(
    Guid OrderId,
    Guid DeliveryPersonId,
    string? DeliveryPersonName) : IRequest<Order?>;

public sealed class ClaimDeliveryOrderCommandHandler(
    IOrderRepository orders,
    IOrderEmailSender emailSender,
    IOrderNotificationPublisher notificationPublisher)
    : IRequestHandler<ClaimDeliveryOrderCommand, Order?>
{
    public async Task<Order?> Handle(
        ClaimDeliveryOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
            return null;
        if (string.IsNullOrWhiteSpace(request.DeliveryPersonName))
            throw new DeliveryClaimForbiddenException();

        order.AssignDeliveryPerson(request.DeliveryPersonId, request.DeliveryPersonName);
        await orders.SaveChangesAsync(cancellationToken);
        await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        await UpdateOrderStatusCommandHandler.SendStatusEmailAsync(order, emailSender, cancellationToken);
        return order;
    }
}

public sealed record UpdateDeliveryLocationCommand(
    Guid OrderId,
    Guid DeliveryPersonId,
    double Latitude,
    double Longitude) : IRequest<Order?>;

public sealed class UpdateDeliveryLocationCommandHandler(IOrderRepository orders)
    : IRequestHandler<UpdateDeliveryLocationCommand, Order?>
{
    public async Task<Order?> Handle(
        UpdateDeliveryLocationCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
            return null;

        order.UpdateDeliveryLocation(request.DeliveryPersonId, request.Latitude, request.Longitude);
        await orders.SaveChangesAsync(cancellationToken);
        return order;
    }
}

public sealed record MarkOrderDeliveredCommand(Guid OrderId, Guid DeliveryPersonId)
    : IRequest<Order?>;

public sealed class MarkOrderDeliveredCommandHandler(
    IOrderRepository orders,
    IOrderEmailSender emailSender,
    IOrderNotificationPublisher notificationPublisher)
    : IRequestHandler<MarkOrderDeliveredCommand, Order?>
{
    public async Task<Order?> Handle(
        MarkOrderDeliveredCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
            return null;

        order.MarkDeliveredBy(request.DeliveryPersonId);
        await orders.SaveChangesAsync(cancellationToken);
        await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        await UpdateOrderStatusCommandHandler.SendStatusEmailAsync(order, emailSender, cancellationToken);
        return order;
    }
}
