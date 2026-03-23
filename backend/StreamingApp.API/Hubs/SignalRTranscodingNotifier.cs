using Microsoft.AspNetCore.SignalR;
using StreamingApp.Application.Interfaces;

namespace StreamingApp.API.Hubs;

/// <summary>
/// Implementação de ITranscodingNotifier que envia eventos via SignalR
/// para o grupo "content-{contentId}", onde o frontend aguarda atualizações.
/// </summary>
public class SignalRTranscodingNotifier : ITranscodingNotifier
{
    private readonly IHubContext<TranscodingHub> _hubContext;

    public SignalRTranscodingNotifier(IHubContext<TranscodingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProgressAsync(Guid contentId, string status, int progressPercent, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"content-{contentId}")
            .SendAsync("TranscodingProgress", new { contentId, status, progressPercent }, ct);
    }

    public async Task NotifyCompletedAsync(Guid contentId, string hlsManifestUrl, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"content-{contentId}")
            .SendAsync("TranscodingCompleted", new { contentId, hlsManifestUrl }, ct);
    }

    public async Task NotifyFailedAsync(Guid contentId, string reason, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"content-{contentId}")
            .SendAsync("TranscodingFailed", new { contentId, reason }, ct);
    }
}
