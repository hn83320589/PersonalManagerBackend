using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Services;

public interface IFileStorageProvider
{
    Task<(string fileUrl, string storedName)> UploadAsync(IFormFile file, string ext);
    Task DeleteAsync(string storedName);
}

// ===== Local storage (dev / fallback) =====
public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _rootPath;

    public LocalFileStorageProvider(IWebHostEnvironment env, IOptions<FileStorageSettings> settings)
    {
        _rootPath = Path.Combine(env.ContentRootPath, settings.Value.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string fileUrl, string storedName)> UploadAsync(IFormFile file, string ext)
    {
        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_rootPath, storedName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return ($"/files/{storedName}", storedName);
    }

    public Task DeleteAsync(string storedName)
    {
        var filePath = Path.Combine(_rootPath, storedName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }
}

// ===== S3-compatible storage (production) =====
public class S3FileStorageProvider : IFileStorageProvider
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageSettings _s3Settings;

    public S3FileStorageProvider(IAmazonS3 s3, IOptions<FileStorageSettings> settings)
    {
        _s3 = s3;
        _s3Settings = settings.Value.S3!;
    }

    public async Task<(string fileUrl, string storedName)> UploadAsync(IFormFile file, string ext)
    {
        var storedName = $"{Guid.NewGuid()}{ext}";
        using var stream = file.OpenReadStream();
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _s3Settings.BucketName,
            Key = storedName,
            InputStream = stream,
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.PublicRead,
        });
        var fileUrl = $"{_s3Settings.PublicBaseUrl.TrimEnd('/')}/{storedName}";
        return (fileUrl, storedName);
    }

    public async Task DeleteAsync(string storedName)
    {
        await _s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _s3Settings.BucketName,
            Key = storedName,
        });
    }
}
