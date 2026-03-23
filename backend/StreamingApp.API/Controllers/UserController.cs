using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Application.Services;
using System.Security.Claims;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly WatchHistoryService _watchHistoryService;

    public UserController(WatchHistoryService watchHistoryService)
    {
        _watchHistoryService = watchHistoryService;
    }

    [HttpGet("watchlist")]
    public async Task<IActionResult> GetWatchlist(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _watchHistoryService.GetWatchlistAsync(userId, ct);
        return Ok(result.Value);
    }

    [HttpPost("watchlist/{contentId}")]
    public async Task<IActionResult> AddToWatchlist(Guid contentId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _watchHistoryService.AddToWatchlistAsync(userId, contentId, ct);
        return Ok(new { message = "Added to watchlist" });
    }

    [HttpDelete("watchlist/{contentId}")]
    public async Task<IActionResult> RemoveFromWatchlist(Guid contentId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _watchHistoryService.RemoveFromWatchlistAsync(userId, contentId, ct);
        if (!result.IsSuccess) return NotFound();
        return NoContent();
    }

    [HttpPost("rating/{contentId}")]
    public async Task<IActionResult> Rate(Guid contentId, [FromBody] RateDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _watchHistoryService.RateContentAsync(userId, contentId, dto.IsLiked, ct);
        return Ok(new { message = "Rating saved" });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _watchHistoryService.GetHistoryAsync(userId, ct);
        return Ok(result.Value);
    }
}

public record RateDto(bool IsLiked);
