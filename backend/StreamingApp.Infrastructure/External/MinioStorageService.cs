using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using StreamingApp.Application.Interfaces;

namespace StreamingApp.Infrastructure.External;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minio;
    private readonly string _bucketName;

    public MinioStorageService(IMinioClient minio, IConfiguration config)
    {
        _minio = minio;
        _bucketName = config["MinIO:BucketName"] ?? "streaming";
    }

    public async Task<string> GeneratePresignedUploadUrlAsync(string objectKey, int expiryMinutes = 60, CancellationToken ct = default)
    {
        var args = new PresignedPutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithExpiry(expiryMinutes * 60);
        return await _minio.PresignedPutObjectAsync(args);
    }

    public async Task<string> GeneratePresignedDownloadUrlAsync(string objectKey, int expiryHours = 24, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithExpiry(expiryHours * 3600);
        return await _minio.PresignedGetObjectAsync(args);
    }

    public async Task UploadFileAsync(string objectKey, Stream stream, string contentType, CancellationToken ct = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);
        await _minio.PutObjectAsync(args, ct);
    }

    public async Task UploadFileAsync(string objectKey, string filePath, string contentType, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        await UploadFileAsync(objectKey, stream, contentType, ct);
    }

    public async Task<bool> FileExistsAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            var args = new StatObjectArgs().WithBucket(_bucketName).WithObject(objectKey);
            await _minio.StatObjectAsync(args, ct);
            return true;
        }
        catch { return false; }
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs().WithBucket(_bucketName).WithObject(objectKey);
        await _minio.RemoveObjectAsync(args, ct);
    }
}
