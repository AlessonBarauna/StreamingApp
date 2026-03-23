using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Application.Interfaces;
using StreamingApp.Application.Jobs;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Interfaces;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/upload")]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase
{
    private readonly IStorageService _storage;
    private readonly IContentRepository _contentRepo;
    private readonly IBackgroundJobClient _jobs;

    public UploadController(IStorageService storage, IContentRepository contentRepo, IBackgroundJobClient jobs)
    {
        _storage = storage;
        _contentRepo = contentRepo;
        _jobs = jobs;
    }

    [HttpPost("presigned")]
    public async Task<IActionResult> GetPresignedUrl([FromBody] PresignedUrlRequest request, CancellationToken ct)
    {
        var key = $"uploads/{Guid.NewGuid()}/{request.FileName}";
        var url = await _storage.GeneratePresignedUploadUrlAsync(key, 60, ct);
        return Ok(new { uploadUrl = url, objectKey = key });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmUpload([FromBody] ConfirmUploadRequest request, CancellationToken ct)
    {
        var content = await _contentRepo.GetByIdAsync(request.ContentId, ct);
        if (content == null) return NotFound();

        content.OriginalFileKey = request.ObjectKey;
        _contentRepo.Update(content);
        await _contentRepo.SaveChangesAsync(ct);

        var jobId = _jobs.Enqueue<TranscodingJob>(j => j.ProcessAsync(content.Id, request.ObjectKey, CancellationToken.None));

        return Ok(new { message = "Transcoding job enqueued", jobId, contentId = content.Id });
    }
}

public record PresignedUrlRequest(string FileName, string ContentType);
public record ConfirmUploadRequest(Guid ContentId, string ObjectKey);
