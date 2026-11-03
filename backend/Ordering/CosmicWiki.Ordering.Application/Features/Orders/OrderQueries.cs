using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Orders;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderDetailsResult?>;

public sealed record OrderDetailsResult(Order Order, DateTime PaymentExpiresAtUtc);

public sealed class GetOrderQueryHandler(
    IOrderRepository orders,
    IOrderLifecycleSettings lifecycleSettings)
    : IRequestHandler<GetOrderQuery, OrderDetailsResult?>
{
    public async Task<OrderDetailsResult?> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(request.OrderId, cancellationToken);
        return order is null
            ? null
            : new(order, order.CreatedAtUtc.AddMinutes(lifecycleSettings.PendingPaymentTimeoutMinutes));
    }
}

public sealed record GetCustomerOrdersQuery(Guid CustomerId)
    : IRequest<IReadOnlyCollection<OrderDetailsResult>>;

public sealed class GetCustomerOrdersQueryHandler(
    IOrderRepository orders,
    IOrderLifecycleSettings lifecycleSettings)
    : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyCollection<OrderDetailsResult>>
{
    public async Task<IReadOnlyCollection<OrderDetailsResult>> Handle(
        GetCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var customerOrders = await orders.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return customerOrders
            .Select(order => new OrderDetailsResult(
                order,
                order.CreatedAtUtc.AddMinutes(lifecycleSettings.PendingPaymentTimeoutMinutes)))
            .ToArray();
    }
}

public sealed record GetTodayOrdersQuery(DateTime StartOfDayUtc)
    : IRequest<IReadOnlyCollection<Order>>;

public sealed class GetTodayOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetTodayOrdersQuery, IReadOnlyCollection<Order>>
{
    public Task<IReadOnlyCollection<Order>> Handle(
        GetTodayOrdersQuery request,
        CancellationToken cancellationToken) =>
        orders.GetCreatedSinceAsync(request.StartOfDayUtc, cancellationToken);
}

public sealed record GetTodayOrdersPageQuery(
    DateTime StartOfDayUtc,
    OrderStatus? Status,
    int Skip,
    int Take) : IRequest<TodayOrdersPageResult>;

public sealed record TodayOrdersPageResult(
    IReadOnlyCollection<Order> Items,
    int TotalCount);

public sealed class GetTodayOrdersPageQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetTodayOrdersPageQuery, TodayOrdersPageResult>
{
    public async Task<TodayOrdersPageResult> Handle(
        GetTodayOrdersPageQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orders.GetCreatedSincePageAsync(
            request.StartOfDayUtc,
            request.Status,
            request.Skip,
            request.Take,
            cancellationToken);
        return new(items, totalCount);
    }
}

public sealed record GetDeliveryOrdersQuery(Guid DeliveryPersonId)
    : IRequest<DeliveryOrdersResult>;

public sealed record DeliveryOrdersResult(
    IReadOnlyCollection<Order> Available,
    IReadOnlyCollection<Order> Assigned);

public sealed class GetDeliveryOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetDeliveryOrdersQuery, DeliveryOrdersResult>
{
    public async Task<DeliveryOrdersResult> Handle(
        GetDeliveryOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var available = await orders.GetAvailableForDeliveryAsync(cancellationToken);
        var assigned = await orders.GetByDeliveryPersonIdAsync(
            request.DeliveryPersonId,
            cancellationToken);
        return new(available, assigned);
    }
}
