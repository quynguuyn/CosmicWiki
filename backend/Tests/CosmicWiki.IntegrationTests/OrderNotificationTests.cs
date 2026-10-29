using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;

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
    public void Latest_migration_exposes_its_target_model()
    {
        var targetEntities = new AddOrderItemDetailsAndNotifications()
            .TargetModel.GetEntityTypes();

        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.OrderLine" &&
            entity.FindProperty("ProductImageUrl") is not null);
        Assert.Contains(targetEntities, entity =>
            entity.Name == "CosmicWiki.Ordering.Domain.Entities.OrderNotification");
    }
}
