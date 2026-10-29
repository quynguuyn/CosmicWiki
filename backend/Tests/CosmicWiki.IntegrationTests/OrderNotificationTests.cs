using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Infrastructure.Persistence.Migrations;
using CosmicWiki.Ordering.Api.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CosmicWiki.IntegrationTests;

public sealed class OrderNotificationTests
{
    [Fact]
    public void Notification_is_unread_until_marked_read_and_mark_read_is_idempotent()
    {
        var notification = OrderNotification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Đơn hàng đã xác nhận",
            "Đơn hàng đang chờ nhân viên nhận giao.",
            "Confirmed");

        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAtUtc);

        notification.MarkRead();
        var firstReadAt = notification.ReadAtUtc;
        notification.MarkRead();

        Assert.True(notification.IsRead);
        Assert.Equal(firstReadAt, notification.ReadAtUtc);
    }

    [Fact]
    public void Notification_can_keep_the_historical_status_timestamp()
    {
        var changedAtUtc = new DateTime(2026, 10, 29, 15, 28, 59, DateTimeKind.Utc);
        var notification = OrderNotification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Đơn hàng đã giao",
            "Đơn hàng đã giao thành công.",
            "Delivered",
            changedAtUtc);

        Assert.Equal(changedAtUtc, notification.CreatedAtUtc);
    }

    [Fact]
    public void Utc_date_json_converter_marks_oracle_timestamps_as_utc()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UtcDateTimeJsonConverter());
        var databaseValue = new DateTime(2026, 10, 29, 23, 8, 0, DateTimeKind.Unspecified);

        var json = JsonSerializer.Serialize(databaseValue, options);
        var restored = JsonSerializer.Deserialize<DateTime>(json, options);

        Assert.EndsWith("Z\"", json);
        Assert.Equal(DateTimeKind.Utc, restored.Kind);
        Assert.Equal(databaseValue.Ticks, restored.Ticks);
    }

    [Fact]
    public void Latest_migration_exposes_its_target_model()
    {
        var targetEntities = new AddUniqueOrderNotificationStatusIndex()
            .TargetModel.GetEntityTypes();

        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.OrderLine" &&
            entity.FindProperty("ProductImageUrl") is not null);
        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.OrderNotification");
        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.Order" &&
            entity.FindProperty("DeliveryAddress") is not null);
        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.OrderNotification" &&
            entity.GetIndexes().Any(index =>
                index.IsUnique &&
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(["CustomerId", "OrderId", "Status"])));
    }
}
