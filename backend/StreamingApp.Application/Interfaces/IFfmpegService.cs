namespace StreamingApp.Application.Interfaces;

public interface IFfmpegService
{
    Task TranscodeToHlsAsync(string inputPath, string outputDir, Guid contentId, CancellationToken ct = default);
    Task ExtractThumbnailAsync(string inputPath, string outputPath, int seekSeconds = 10, CancellationToken ct = default);
    Task<string> GenerateMasterPlaylistAsync(string outputDir, CancellationToken ct = default);
}
