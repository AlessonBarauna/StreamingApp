using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StreamingApp.Application.Interfaces;

namespace StreamingApp.Infrastructure.External;

public class FfmpegService : IFfmpegService
{
    private readonly string _ffmpegPath;
    private readonly ILogger<FfmpegService> _logger;

    public FfmpegService(IConfiguration config, ILogger<FfmpegService> logger)
    {
        _ffmpegPath = config["FfmpegPath"] ?? "ffmpeg";
        _logger = logger;
    }

    public async Task TranscodeToHlsAsync(string inputPath, string outputDir, Guid contentId, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDir);

        var profiles = new[]
        {
            ("1080p", "1920:1080", "4000k", "4200k", "8000k", "192k"),
            ("720p",  "1280:720",  "2000k", "2100k", "4000k", "128k"),
            ("480p",  "854:480",   "800k",  "850k",  "1600k", "96k"),
            ("360p",  "640:360",   "400k",  "420k",  "800k",  "64k"),
        };

        foreach (var (name, scale, vb, maxrate, bufsize, ab) in profiles)
        {
            ct.ThrowIfCancellationRequested();
            var segmentPattern = Path.Combine(outputDir, $"{name}_%04d.ts");
            var playlist = Path.Combine(outputDir, $"{name}.m3u8");

            var args = $"-i \"{inputPath}\" -vf scale={scale} -c:v libx264 -b:v {vb} -maxrate {maxrate} -bufsize {bufsize} " +
                       $"-c:a aac -b:a {ab} -ar 48000 " +
                       $"-hls_time 6 -hls_playlist_type vod " +
                       $"-hls_segment_filename \"{segmentPattern}\" \"{playlist}\" -y";

            _logger.LogInformation("Transcoding {ContentId} to {Profile}", contentId, name);
            await RunFfmpegAsync(args, ct);
        }
    }

    public async Task ExtractThumbnailAsync(string inputPath, string outputPath, int seekSeconds = 10, CancellationToken ct = default)
    {
        var time = TimeSpan.FromSeconds(seekSeconds).ToString(@"hh\:mm\:ss");
        var args = $"-i \"{inputPath}\" -ss {time} -vframes 1 -vf scale=1280:720 \"{outputPath}\" -y";
        await RunFfmpegAsync(args, ct);
    }

    public Task<string> GenerateMasterPlaylistAsync(string outputDir, CancellationToken ct = default)
    {
        var content = """
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-STREAM-INF:BANDWIDTH=4192000,RESOLUTION=1920x1080,CODECS="avc1.64001f,mp4a.40.2"
1080p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=2128000,RESOLUTION=1280x720,CODECS="avc1.64001f,mp4a.40.2"
720p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=896000,RESOLUTION=854x480,CODECS="avc1.4d001e,mp4a.40.2"
480p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=464000,RESOLUTION=640x360,CODECS="avc1.42001e,mp4a.40.2"
360p.m3u8
""";
        var masterPath = Path.Combine(outputDir, "master.m3u8");
        File.WriteAllText(masterPath, content);
        return Task.FromResult(masterPath);
    }

    private async Task RunFfmpegAsync(string arguments, CancellationToken ct)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var error = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            _logger.LogError("FFmpeg failed: {Error}", error);
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
        }
    }
}
