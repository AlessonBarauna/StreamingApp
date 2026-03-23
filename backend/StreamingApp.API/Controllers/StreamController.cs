using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Application.DTOs.Stream;
using StreamingApp.Application.Interfaces;
using StreamingApp.Application.Services;
using StreamingApp.Domain.Interfaces;
using StreamingApp.Domain.Entities;
using System.Security.Claims;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/stream")]
[Authorize]
public class StreamController : ControllerBase
{
    private readonly IContentRepository _contentRepo;
    private readonly IStorageService _storage;
    private readonly WatchHistoryService _watchHistory;

    public StreamController(IContentRepository contentRepo, IStorageService storage, WatchHistoryService watchHistory)
    {
        _contentRepo = contentRepo;
        _storage = storage;
        _watchHistory = watchHistory;
    }

    [HttpGet("{contentId}/manifest")]
    public async Task<IActionResult> GetManifest(Guid contentId, CancellationToken ct)
    {
        var content = await _contentRepo.GetByIdAsync(contentId, ct);
        if (content == null) return NotFound();
        if (content.HlsManifestUrl == null) return BadRequest(new { error = "Conteúdo não está disponível para streaming." });

        var objectKey = $"conteudo/{contentId}/hls/master.m3u8";
        var url = await _storage.GeneratePresignedDownloadUrlAsync(objectKey, 24, ct);
        return Redirect(url);
    }

    [HttpGet("{contentId}/progress")]
    public async Task<IActionResult> GetProgress(Guid contentId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _watchHistory.GetProgressAsync(userId, contentId, ct);
        return Ok(result.Value);
    }

    [HttpPost("{contentId}/progress")]
    public async Task<IActionResult> SaveProgress(Guid contentId, SaveProgressDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _watchHistory.SaveProgressAsync(userId, contentId, dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { message = "Progress saved" });
    }
}
