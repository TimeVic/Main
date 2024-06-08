using Api.Requests.Abstractions;

namespace TimeTracker.Api.FileStorage.Dto.Entities;

public class FileStorageFileDto: IResponse
{
    public required string Id { get; set; }
    public string? Directory { get; set; }
    public required string BucketName { get; set; }
    public required string FileName { get; set; }
    public required string MimeType { get; set; }
    public required string Extension { get; set; }
    public required long Size { get; set; }

    public string PublicUrl => $"/{BucketName}/{Id}.{Extension}";
}
