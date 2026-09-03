using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Infrastructure.Services.DocumentImages.Minio;

namespace YaeaY.Account.Infrastructure.Services.DocumentImages.Local;

public sealed class LocalFileSystemDocumentImageStorage(
    IOptions<MinioDocumentImageStorageOptions> options,
    IHostEnvironment environment) : IDocumentImageStorage
{
    private readonly MinioDocumentImageStorageOptions _options = options.Value;
    private readonly string _rootPath = Path.GetFullPath(options.Value.LocalRootPath, environment.ContentRootPath);

    public async Task<StoredDocumentImage> StoreCpfImageAsync(
        Guid userId,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken)
    {
        var normalizedFileName = Path.GetFileName(originalFileName.Trim());
        var extension = Path.GetExtension(normalizedFileName).ToLowerInvariant();
        var objectKey = $"users/{userId:N}/cpf/{Guid.NewGuid():N}{extension}";
        var path = ResolvePath(objectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long written = 0;
        int read;
        while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            hasher.AppendData(buffer, 0, read);
            written += read;
        }

        if (written != fileSizeBytes)
        {
            await destination.DisposeAsync();
            File.Delete(path);
            throw new InvalidOperationException("The uploaded document image size does not match the request metadata.");
        }

        var hash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
        return new StoredDocumentImage(objectKey, normalizedFileName, contentType, written, hash);
    }

    public Task<bool> ExistsAsync(string storageObjectKey, CancellationToken cancellationToken) =>
        Task.FromResult(File.Exists(ResolvePath(storageObjectKey)));

    public Task<Stream?> OpenReadAsync(string storageObjectKey, CancellationToken cancellationToken)
    {
        var path = ResolvePath(storageObjectKey);
        Stream? content = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null;
        return Task.FromResult(content);
    }

    public Task DeleteAsync(string storageObjectKey, CancellationToken cancellationToken)
    {
        var path = ResolvePath(storageObjectKey);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string storageObjectKey)
    {
        if (string.IsNullOrWhiteSpace(storageObjectKey) || !storageObjectKey.StartsWith("users/", StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid document image storage key.");

        var path = Path.GetFullPath(Path.Combine(_rootPath, storageObjectKey.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Document image storage key resolves outside the configured root.");

        return path;
    }
}
