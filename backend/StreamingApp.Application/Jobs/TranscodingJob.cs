using Hangfire;
using Microsoft.Extensions.Logging;
using StreamingApp.Application.Interfaces;
using StreamingApp.Domain.Enums;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.Application.Jobs;

public class TranscodingJob
{
    private readonly IFfmpegService _ffmpeg;
    private readonly IStorageService _storage;
    private readonly IContentRepository _contentRepo;
    private readonly ITranscodingNotifier _notifier;
    private readonly ILogger<TranscodingJob> _logger;

    public TranscodingJob(
        IFfmpegService ffmpeg,
        IStorageService storage,
        IContentRepository contentRepo,
        ITranscodingNotifier notifier,
        ILogger<TranscodingJob> logger)
    {
        _ffmpeg = ffmpeg;
        _storage = storage;
        _contentRepo = contentRepo;
        _notifier = notifier;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task ProcessAsync(Guid contentId, string originalFileKey, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting transcoding for content {ContentId}", contentId);

        var content = await _contentRepo.GetByIdAsync(contentId, ct);
        if (content is null)
        {
            _logger.LogError("Content {ContentId} not found", contentId);
            return;
        }

        content.Status = TranscodingStatus.Processing;
        _contentRepo.Update(content);
        await _contentRepo.SaveChangesAsync(ct);

        await _notifier.NotifyProgressAsync(contentId, "processing", 0, ct);

        var tmpDir = Path.Combine("/tmp/transcoding", contentId.ToString());
        var hlsDir = Path.Combine(tmpDir, "hls");
        var inputFile = Path.Combine(tmpDir, "input.mp4");

        try
        {
            Directory.CreateDirectory(hlsDir);

            _logger.LogInformation("Downloading original file for content {ContentId}", contentId);
            await _notifier.NotifyProgressAsync(contentId, "downloading", 10, ct);
            // TODO: implementar download do arquivo original do MinIO para inputFile
            // await _storage.DownloadFileAsync(originalFileKey, inputFile, ct);

            _logger.LogInformation("Transcoding content {ContentId}", contentId);
            await _notifier.NotifyProgressAsync(contentId, "transcoding", 30, ct);
            await _ffmpeg.TranscodeToHlsAsync(inputFile, hlsDir, contentId, ct);

            await _notifier.NotifyProgressAsync(contentId, "generating_playlist", 70, ct);
            await _ffmpeg.GenerateMasterPlaylistAsync(hlsDir, ct);

            var thumbPath = Path.Combine(tmpDir, "thumbnail.jpg");
            await _ffmpeg.ExtractThumbnailAsync(inputFile, thumbPath, 10, ct);

            _logger.LogInformation("Uploading HLS segments for content {ContentId}", contentId);
            await _notifier.NotifyProgressAsync(contentId, "uploading", 80, ct);

            var masterKey = $"conteudo/{contentId}/hls/master.m3u8";
            await _storage.UploadFileAsync(masterKey, Path.Combine(hlsDir, "master.m3u8"), "application/vnd.apple.mpegurl", ct);

            foreach (var file in Directory.GetFiles(hlsDir))
            {
                var fileName = Path.GetFileName(file);
                var key = $"conteudo/{contentId}/hls/{fileName}";
                var mime = file.EndsWith(".m3u8") ? "application/vnd.apple.mpegurl" : "video/mp2t";
                await _storage.UploadFileAsync(key, file, mime, ct);
            }

            if (File.Exists(thumbPath))
                await _storage.UploadFileAsync($"conteudo/{contentId}/thumbnail.jpg", thumbPath, "image/jpeg", ct);

            var hlsManifestUrl = $"/hls/{contentId}/hls/master.m3u8";
            content.Status = TranscodingStatus.Ready;
            content.HlsManifestUrl = hlsManifestUrl;
            content.ThumbnailUrl = $"/hls/{contentId}/thumbnail.jpg";
            _contentRepo.Update(content);
            await _contentRepo.SaveChangesAsync(ct);

            _logger.LogInformation("Transcoding completed for content {ContentId}", contentId);
            await _notifier.NotifyCompletedAsync(contentId, hlsManifestUrl, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcoding failed for content {ContentId}", contentId);
            content.Status = TranscodingStatus.Failed;
            _contentRepo.Update(content);
            await _contentRepo.SaveChangesAsync(ct);

            await _notifier.NotifyFailedAsync(contentId, ex.Message, ct);
            throw;
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }
}
