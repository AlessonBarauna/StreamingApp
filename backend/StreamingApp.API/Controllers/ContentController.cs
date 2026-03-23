using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Application.DTOs.Content;
using StreamingApp.Application.Services;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/content")]
public class ContentController : ControllerBase
{
    private readonly ContentService _contentService;

    public ContentController(ContentService contentService) => _contentService = contentService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] Guid? category = null, [FromQuery] string? search = null, [FromQuery] string? type = null, CancellationToken ct = default)
    {
        var result = await _contentService.GetPagedAsync(page, limit, category, search, type, ct);
        return Ok(result.Value);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        var result = await _contentService.GetFeaturedAsync(ct);
        return Ok(result.Value);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending(CancellationToken ct)
    {
        var result = await _contentService.GetTrendingAsync(ct);
        return Ok(result.Value);
    }

    [HttpGet("new-releases")]
    public async Task<IActionResult> GetNewReleases(CancellationToken ct)
    {
        var result = await _contentService.GetNewReleasesAsync(ct);
        return Ok(result.Value);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var result = await _contentService.GetPagedAsync(1, 20, null, q, null, ct);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _contentService.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id}/episodes")]
    public async Task<IActionResult> GetEpisodes(Guid id, CancellationToken ct)
    {
        var result = await _contentService.GetEpisodesAsync(id, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateContentDto dto, CancellationToken ct)
    {
        var result = await _contentService.CreateAsync(dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateContentDto dto, CancellationToken ct)
    {
        var result = await _contentService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _contentService.DeleteAsync(id, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }

    // --- Episode sub-resource (Admin only) ---

    [Authorize(Roles = "Admin")]
    [HttpPost("{contentId}/episodes")]
    public async Task<IActionResult> CreateEpisode(Guid contentId, CreateEpisodeDto dto, CancellationToken ct)
    {
        var result = await _contentService.CreateEpisodeAsync(contentId, dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetEpisodes), new { id = contentId }, result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{contentId}/episodes/{episodeId}")]
    public async Task<IActionResult> UpdateEpisode(Guid contentId, Guid episodeId, UpdateEpisodeDto dto, CancellationToken ct)
    {
        var result = await _contentService.UpdateEpisodeAsync(contentId, episodeId, dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{contentId}/episodes/{episodeId}")]
    public async Task<IActionResult> DeleteEpisode(Guid contentId, Guid episodeId, CancellationToken ct)
    {
        var result = await _contentService.DeleteEpisodeAsync(contentId, episodeId, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }
}
