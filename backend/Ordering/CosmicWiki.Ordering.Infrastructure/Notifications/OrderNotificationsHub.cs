using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CosmicWiki.Ordering.Infrastructure.Notifications;

[Authorize]
public sealed class OrderNotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var customerId = Context.User?.FindFirst("sub")?.Value;
        if (!Guid.TryParse(customerId, out var userId) || userId == Guid.Empty)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId:D}");
        await base.OnConnectedAsync();
    }
}
