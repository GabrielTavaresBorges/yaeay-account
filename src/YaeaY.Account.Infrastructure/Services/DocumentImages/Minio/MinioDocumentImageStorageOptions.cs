namespace YaeaY.Account.Infrastructure.Services.DocumentImages.Minio;

public sealed class MinioDocumentImageStorageOptions
{
    public const string SectionName = "DocumentImageStorage";

    public bool Enabled { get; init; }
    public string Endpoint { get; init; } = "minio:9000";
    public bool UseSsl { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "yaeay-account-documents";
}
