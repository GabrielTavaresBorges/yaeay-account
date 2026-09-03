using System.Security.Cryptography;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;

namespace YaeaY.Account.Infrastructure.Services.DocumentImages.Minio;

public sealed class MinioDocumentImageStorage : IDocumentImageStorage
{
    private readonly MinioDocumentImageStorageOptions _options;
    private IMinioClient? _client;

    public MinioDocumentImageStorage(IOptions<MinioDocumentImageStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StoredDocumentImage> StoreCpfImageAsync(
        Guid userId,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken)
    {
        EnsureEnabled();
        var normalizedFileName = Path.GetFileName(originalFileName.Trim());
        var extension = Path.GetExtension(normalizedFileName).ToLowerInvariant();
        var objectKey = $"users/{userId:N}/cpf/{Guid.NewGuid():N}{extension}";

        await using var bufferedContent = new MemoryStream();
        await content.CopyToAsync(bufferedContent, cancellationToken);
        if (bufferedContent.Length != fileSizeBytes)
            throw new InvalidOperationException("The uploaded document image size does not match the request metadata.");

        var hash = Convert.ToHexString(SHA256.HashData(bufferedContent.GetBuffer().AsSpan(0, checked((int)bufferedContent.Length))).ToArray()).ToLowerInvariant();
        bufferedContent.Position = 0;

        await EnsureBucketAsync(cancellationToken);
        await Client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(bufferedContent)
            .WithObjectSize(bufferedContent.Length)
            .WithContentType(contentType), cancellationToken);

        return new StoredDocumentImage(objectKey, normalizedFileName, contentType, fileSizeBytes, hash);
    }

    public async Task<bool> ExistsAsync(string storageObjectKey, CancellationToken cancellationToken)
    {
        EnsureEnabled();
        try
        {
            await Client.StatObjectAsync(new StatObjectArgs().WithBucket(_options.BucketName).WithObject(storageObjectKey), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream?> OpenReadAsync(string storageObjectKey, CancellationToken cancellationToken)
    {
        EnsureEnabled();
        var result = new MemoryStream();
        try
        {
            await Client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageObjectKey)
                .WithCallbackStream(stream => stream.CopyTo(result)), cancellationToken);
            result.Position = 0;
            return result;
        }
        catch
        {
            await result.DisposeAsync();
            return null;
        }
    }

    public async Task DeleteAsync(string storageObjectKey, CancellationToken cancellationToken)
    {
        EnsureEnabled();
        await Client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageObjectKey), cancellationToken);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var bucketExists = await Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.BucketName), cancellationToken);
        if (!bucketExists)
            await Client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.BucketName), cancellationToken);
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("Document image storage is disabled.");
    }

    private IMinioClient Client => _client ??= new MinioClient()
        .WithEndpoint(_options.Endpoint)
        .WithCredentials(_options.AccessKey, _options.SecretKey)
        .WithSSL(_options.UseSsl)
        .Build();
}
