namespace StreamingApp.Application.Interfaces;

public interface IStorageService
{
    Task<string> GeneratePresignedUploadUrlAsync(string objectKey, int expiryMinutes = 60, CancellationToken ct = default);
    Task<string> GeneratePresignedDownloadUrlAsync(string objectKey, int expiryHours = 24, CancellationToken ct = default);
    Task UploadFileAsync(string objectKey, Stream stream, string contentType, CancellationToken ct = default);
    Task UploadFileAsync(string objectKey, string filePath, string contentType, CancellationToken ct = default);
    Task<bool> FileExistsAsync(string objectKey, CancellationToken ct = default);
    Task DeleteFileAsync(string objectKey, CancellationToken ct = default);
}
