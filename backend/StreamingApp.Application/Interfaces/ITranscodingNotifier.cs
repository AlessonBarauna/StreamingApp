namespace StreamingApp.Application.Interfaces;

/// <summary>
/// Abstração para envio de notificações de progresso de transcoding em tempo real.
/// Implementada na camada API via SignalR (IHubContext), mantendo Application
/// desacoplada de qualquer tecnologia de transporte.
/// </summary>
public interface ITranscodingNotifier
{
    Task NotifyProgressAsync(Guid contentId, string status, int progressPercent, CancellationToken ct = default);
    Task NotifyCompletedAsync(Guid contentId, string hlsManifestUrl, CancellationToken ct = default);
    Task NotifyFailedAsync(Guid contentId, string reason, CancellationToken ct = default);
}
