using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StreamingApp.API.Hubs;

[Authorize]
public class TranscodingHub : Hub
{
    public async Task JoinContentGroup(string contentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"content-{contentId}");
    }

    public async Task LeaveContentGroup(string contentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"content-{contentId}");
    }
}
